/**
 * github-push.mjs
 * Push project files to a GitHub repository via the Git Data API.
 *
 * Usage:
 *   node scripts/github-push.mjs                          # push to default repo
 *   node scripts/github-push.mjs --name my-repo          # push to a named repo (creates it if absent)
 *   node scripts/github-push.mjs --name my-repo --private # create as private repo
 *
 * Options:
 *   --name <repo>     Target repository name (default: juneairwaysresafeserver)
 *   --private         Create repo as private (only applies if repo is being created)
 *   --description     Repository description string
 */

import { ReplitConnectors } from "@replit/connectors-sdk";
import fs from "fs";
import path from "path";

// ── CLI arg parsing ────────────────────────────────────────────────────────────
const args = process.argv.slice(2);
function getArg(flag) {
  const idx = args.indexOf(flag);
  return idx !== -1 && args[idx + 1] ? args[idx + 1] : null;
}
const isPrivate = args.includes("--private");
const OWNER = "daluvalanokia";
const REPO = getArg("--name") || "juneairwaysresafeserver";
const DESCRIPTION = getArg("--description") || "AirwaysMergeSafeServer — airways safe server project";

console.log(`Target repo : ${OWNER}/${REPO}`);
console.log(`Visibility  : ${isPrivate ? "private" : "public"}`);

// ── GitHub API helper ──────────────────────────────────────────────────────────
const connectors = new ReplitConnectors();

async function ghApi(endpoint, options = {}) {
  const resp = await connectors.proxy("github", endpoint, options);
  const data = await resp.json();
  // Tolerate expected "already exists" errors
  if (data.message && data.message !== "Repository creation failed." ) {
    const ignore = ["Not Found", "Branch not found"].some(m => data.message.includes(m));
    if (!ignore) {
      // surface real errors but don't throw on ref-update conflicts we handle
    }
  }
  return data;
}

async function ghPost(endpoint, body) {
  return ghApi(endpoint, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
}

async function ghPatch(endpoint, body) {
  return ghApi(endpoint, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
}

// ── File collection ────────────────────────────────────────────────────────────
const ROOT_FILES = [
  "package.json",
  "pnpm-workspace.yaml",
  "tsconfig.json",
  "tsconfig.base.json",
  "replit.md",
  "replit.nix",
];

const SKIP_EXTENSIONS = new Set([".db", ".db-shm", ".db-wal"]);
const SKIP_DIRS = new Set(["obj", "bin", "node_modules", ".git"]);

function collectFiles(dir, base = "") {
  const results = [];
  for (const entry of fs.readdirSync(dir)) {
    if (SKIP_DIRS.has(entry)) continue;
    const full = path.join(dir, entry);
    const rel = base ? `${base}/${entry}` : entry;
    const stat = fs.statSync(full);
    if (stat.isDirectory()) {
      results.push(...collectFiles(full, rel));
    } else {
      if (!SKIP_EXTENSIONS.has(path.extname(entry).toLowerCase())) {
        results.push({ full, rel });
      }
    }
  }
  return results;
}

async function createBlob(filePath, retries = 5) {
  const buf = fs.readFileSync(filePath);
  const isBinary = buf.slice(0, 8192).includes(0x00);
  const encoding = isBinary ? "base64" : "utf-8";
  const content = isBinary ? buf.toString("base64") : buf.toString("utf-8");
  for (let attempt = 1; attempt <= retries; attempt++) {
    const blob = await ghPost(`/repos/${OWNER}/${REPO}/git/blobs`, { content, encoding });
    if (blob.sha) return blob.sha;
    const isRateLimit = JSON.stringify(blob).includes("Rate limit");
    if (isRateLimit && attempt < retries) {
      const wait = attempt * 1500;
      process.stdout.write(`[rate-limit, retry ${attempt}/${retries} in ${wait}ms]`);
      await new Promise(r => setTimeout(r, wait));
    } else {
      throw new Error(`No SHA returned for ${filePath}: ${JSON.stringify(blob)}`);
    }
  }
}

// ── Main ───────────────────────────────────────────────────────────────────────
async function main() {
  // 1. Ensure the repo exists (create if missing)
  const repoInfo = await ghApi(`/repos/${OWNER}/${REPO}`);
  if (repoInfo.message === "Not Found") {
    console.log("Repository not found — creating it...");
    const created = await ghPost("/user/repos", {
      name: REPO,
      description: DESCRIPTION,
      private: isPrivate,
      auto_init: true,
    });
    if (!created.full_name) {
      console.error("Failed to create repo:", JSON.stringify(created, null, 2));
      process.exit(1);
    }
    console.log("Created:", created.html_url);
    // Small delay for GitHub to initialise the default branch
    await new Promise(r => setTimeout(r, 2000));
  } else {
    console.log("Repository exists:", repoInfo.html_url);
  }

  // 2. Collect files
  const files = [];
  for (const f of ROOT_FILES) {
    if (fs.existsSync(f)) files.push({ full: f, rel: f });
  }
  if (fs.existsSync("scripts")) {
    for (const entry of fs.readdirSync("scripts")) {
      if (entry === "github-push.mjs") continue; // skip self
      if (/\.(sh|js|mjs|ts)$/.test(entry)) {
        files.push({ full: `scripts/${entry}`, rel: `scripts/${entry}` });
      }
    }
  }
  files.push(...collectFiles("artifacts/airways-mergesafe", "artifacts/airways-mergesafe"));
  console.log(`Files to push: ${files.length}`);

  // 3. Get HEAD commit of main branch
  const refData = await ghApi(`/repos/${OWNER}/${REPO}/git/ref/heads/main`);
  if (!refData.object) {
    console.error("Could not resolve main branch ref:", JSON.stringify(refData));
    process.exit(1);
  }
  const baseCommitSha = refData.object.sha;
  const baseCommit = await ghApi(`/repos/${OWNER}/${REPO}/git/commits/${baseCommitSha}`);
  const baseTreeSha = baseCommit.tree.sha;
  console.log(`Base commit : ${baseCommitSha.slice(0, 7)}  tree: ${baseTreeSha.slice(0, 7)}`);

  // 4. Create blobs in parallel batches of 3 (keeps well under 10 RPS limit)
  const BATCH = 3;
  const treeItems = [];
  for (let i = 0; i < files.length; i += BATCH) {
    const batch = files.slice(i, i + BATCH);
    const results = await Promise.all(
      batch.map(async ({ full, rel }) => {
        try {
          const sha = await createBlob(full);
          process.stdout.write(".");
          return { path: rel, mode: "100644", type: "blob", sha };
        } catch (e) {
          console.error(`\nBlob failed for ${rel}: ${e.message}`);
          return null;
        }
      })
    );
    treeItems.push(...results.filter(Boolean));
  }
  console.log(`\nBlobs created: ${treeItems.length}`);

  // 5. Create tree → commit → update ref
  const newTree = await ghPost(`/repos/${OWNER}/${REPO}/git/trees`, {
    base_tree: baseTreeSha,
    tree: treeItems,
  });
  console.log("New tree    :", newTree.sha?.slice(0, 7));

  const newCommit = await ghPost(`/repos/${OWNER}/${REPO}/git/commits`, {
    message: `Push: AirwaysMergeSafeServer project (${new Date().toISOString().slice(0, 10)})`,
    tree: newTree.sha,
    parents: [baseCommitSha],
  });
  console.log("New commit  :", newCommit.sha?.slice(0, 7));

  const updated = await ghPatch(`/repos/${OWNER}/${REPO}/git/refs/heads/main`, {
    sha: newCommit.sha,
    force: false,
  });
  if (updated.object) {
    console.log("\nDone! https://github.com/" + OWNER + "/" + REPO);
  } else {
    console.error("Ref update failed:", JSON.stringify(updated));
  }
}

main().catch(e => { console.error(e); process.exit(1); });
