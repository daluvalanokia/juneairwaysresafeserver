using System.Net.Http.Headers;
using System.Text.Json;
using AirwaysMergeSafeClient.Data;
using AirwaysMergeSafeClient.Models;
using AirwaysMergeSafeClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeClient.Controllers;

public class SettingsController : Controller
{
    private readonly AppDbContext        _db;
    private readonly LiveDataCache       _cache;
    private readonly IHttpClientFactory  _http;

    public SettingsController(AppDbContext db, LiveDataCache cache, IHttpClientFactory http)
    {
        _db    = db;
        _cache = cache;
        _http  = http;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cfg = await _db.ClientConfig.FirstOrDefaultAsync() ?? new ClientConfig();
        ViewBag.VehicleName = string.IsNullOrWhiteSpace(cfg.AutoDisplayName)
            ? "Vehicle Client" : cfg.AutoDisplayName;
        ViewBag.Status = _cache.Status.ToString();
        return View(cfg);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ClientConfig model)
    {
        var existing = await _db.ClientConfig.FirstOrDefaultAsync();
        if (existing is null)
        {
            // Generate VehicleId in required format if blank
            if (string.IsNullOrWhiteSpace(model.VehicleId))
                model.VehicleId = GenerateVehicleId(model.AutoMake, model.AutoModel);
            model.UpdatedDate = DateTime.UtcNow;
            _db.ClientConfig.Add(model);
        }
        else
        {
            // Preserve auto-generated IDs if user left them blank
            if (string.IsNullOrWhiteSpace(model.VehicleId))
                model.VehicleId = string.IsNullOrWhiteSpace(existing.VehicleId)
                    ? GenerateVehicleId(model.AutoMake, model.AutoModel)
                    : existing.VehicleId;
            if (string.IsNullOrWhiteSpace(model.DeviceId))  model.DeviceId  = existing.DeviceId;
            model.Id          = existing.Id;
            model.UpdatedDate = DateTime.UtcNow;
            _db.Entry(existing).CurrentValues.SetValues(model);
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }

    private static string GenerateVehicleId(string make, string model)
    {
        var m  = string.IsNullOrWhiteSpace(make)  ? "VH" : make.Trim().Replace(" ", "");
        var mo = string.IsNullOrWhiteSpace(model) ? "XX" : model.Trim().Replace(" ", "");
        var id = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"VH-{m}-{mo}-{id}";
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ServerBaseUrl) || string.IsNullOrWhiteSpace(req.DeviceApiKey))
            return Json(new { ok = false, message = "Server URL and API key are required." });

        // SSRF guard: only allow http/https schemes
        if (!Uri.TryCreate(req.ServerBaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            return Json(new { ok = false, message = "Server URL must use http:// or https://." });

        try
        {
            using var http = _http.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            http.DefaultRequestHeaders.Add("X-Device-Token", req.DeviceApiKey);

            var hw  = Uri.EscapeDataString(req.HighwayId ?? "");
            var url = $"{req.ServerBaseUrl.TrimEnd('/')}/api/stats?highwayId={hw}";
            var resp = await http.GetAsync(url);

            if (resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<JsonElement>(body);
                return Json(new { ok = true, message = $"Connected — events: {stats.GetProperty("events")}, zones: {stats.GetProperty("zones")}" });
            }
            return Json(new { ok = false, message = $"Server returned {(int)resp.StatusCode} {resp.ReasonPhrase}" });
        }
        catch (TaskCanceledException)
        {
            return Json(new { ok = false, message = "Connection timed out after 10 s." });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, message = ex.Message });
        }
    }

    public sealed class TestConnectionRequest
    {
        public string? ServerBaseUrl { get; set; }
        public string? DeviceApiKey  { get; set; }
        public string? HighwayId     { get; set; }
    }
}
