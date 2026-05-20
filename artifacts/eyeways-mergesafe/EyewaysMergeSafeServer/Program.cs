using EyewaysMergeSafeServer.Data;
using EyewaysMergeSafeServer.Filters;
using EyewaysMergeSafeServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var tomTomKeyFile = Path.Combine(AppContext.BaseDirectory, "tomtomkey.json");
if (File.Exists(tomTomKeyFile))
    builder.Configuration.AddJsonFile(tomTomKeyFile, optional: true, reloadOnChange: true);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var npgsqlConnStr = ParsePostgresUrl(databaseUrl);
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(npgsqlConnStr));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

static string ParsePostgresUrl(string url)
{
    try
    {
        var m = System.Text.RegularExpressions.Regex.Match(url,
            @"^(?:postgresql|postgres)://([^:@]+)(?::([^@]*))?@([^/:?]+)(?::(\d+))?/([^?]*)(?:\?(.*))?$");
        if (!m.Success) return url;

        var user = m.Groups[1].Value;
        var pass = m.Groups[2].Value;
        var host = m.Groups[3].Value;
        var port = m.Groups[4].Success ? m.Groups[4].Value : "5432";
        var db   = m.Groups[5].Value;
        var qs   = m.Groups[6].Value;

        var conn = $"Host={host};Port={port};Database={db};Username={user};Password={pass};";
        if (!string.IsNullOrEmpty(qs))
        {
            foreach (var param in qs.Split('&'))
            {
                var kv = param.Split('=', 2);
                if (kv.Length == 2 && kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                    conn += $"SSL Mode={kv[1]};";
            }
        }
        return conn;
    }
    catch { return url; }
}

builder.Services.AddControllersWithViews(opts =>
{
    opts.Filters.Add<SessionAuthFilter>();
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout    = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly   = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(opts =>
{
    opts.AddPolicy("Highways",   p => p.Expire(TimeSpan.FromMinutes(10)).Tag("highways"));
    opts.AddPolicy("ShortLive",  p => p.Expire(TimeSpan.FromMinutes(5)));
});

builder.Services.AddHttpClient();

builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
});

// ── Application services ──────────────────────────────────────────────────
builder.Services.AddScoped<InputPayloadService>();
builder.Services.AddSingleton<TrafficService>();
builder.Services.AddSingleton<ConfigService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        var pending = db.Database.GetPendingMigrations();
        if (pending.Any())
            db.Database.Migrate();
        else
            db.Database.EnsureCreated();
    }
    catch
    {
        db.Database.EnsureCreated();
    }
    DbInitializer.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseOutputCache();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Portal}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
