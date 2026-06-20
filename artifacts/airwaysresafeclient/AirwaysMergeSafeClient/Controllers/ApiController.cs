using System.Text;
using System.Text.Json;
using AirwaysMergeSafeClient.Data;
using AirwaysMergeSafeClient.Models;
using AirwaysMergeSafeClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeClient.Controllers;

[Route("api")]
public class ApiController : Controller
{
    private readonly AppDbContext       _db;
    private readonly LiveDataCache      _cache;
    private readonly IHttpClientFactory _http;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiController(AppDbContext db, LiveDataCache cache, IHttpClientFactory http)
    {
        _db    = db;
        _cache = cache;
        _http  = http;
    }

    // ── GET /api/live-snapshot ────────────────────────────────────────────
    [HttpGet("live-snapshot")]
    public IActionResult LiveSnapshot() => Json(_cache.ToSnapshot());

    // ── POST /api/simulate ────────────────────────────────────────────────
    /// <summary>
    /// Accepts a simulation step from the browser, forwards it to the server's
    /// POST /api/events/ingest, saves the result to SimLog, and returns the
    /// server's classification response.
    /// </summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateRequest req)
    {
        var cfg = await _db.ClientConfig.AsNoTracking().FirstOrDefaultAsync();
        if (cfg is null || string.IsNullOrWhiteSpace(cfg.ServerBaseUrl))
            return Json(new { ok = false, error = "Server not configured. Go to Settings first." });
        if (string.IsNullOrWhiteSpace(cfg.DeviceApiKey))
            return Json(new { ok = false, error = "Device API Key not set. Go to Settings first." });

        // Update stored position so GPS tab reflects simulation location
        var live = await _db.ClientConfig.FirstOrDefaultAsync();
        if (live is not null)
        {
            live.CurrentLatitude  = req.Latitude;
            live.CurrentLongitude = req.Longitude;
            live.CurrentSpeedMph  = req.SpeedMph;
            live.CurrentHeading   = req.Heading;
            await _db.SaveChangesAsync();
        }

        var payload = JsonSerializer.Serialize(new
        {
            eventType      = req.EventType ?? cfg.DefaultEventType,
            highwayId      = cfg.HighwayId,
            vehicleId      = cfg.VehicleId,
            deviceId       = cfg.DeviceId,
            vehicleType    = cfg.AutoType,
            isAirFlyCar    = req.IsAirFlyCar ?? cfg.IsAirFlyCar,
            speedMph       = req.SpeedMph,
            latitude       = req.Latitude,
            longitude      = req.Longitude,
            altitudeMeters = req.AltitudeMeters
        });

        var log = new SimLog
        {
            VehicleId      = cfg.VehicleId,
            EventType      = req.EventType ?? cfg.DefaultEventType,
            HighwayId      = cfg.HighwayId,
            Latitude       = req.Latitude,
            Longitude      = req.Longitude,
            AltitudeMeters = req.AltitudeMeters,
            SpeedMph       = req.SpeedMph,
            IsAirFlyCar    = req.IsAirFlyCar ?? "N",
            CreatedDate    = DateTime.UtcNow
        };

        try
        {
            using var http = _http.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(cfg.HttpTimeoutSeconds);
            http.DefaultRequestHeaders.Add("X-Device-Token", cfg.DeviceApiKey);

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp    = await http.PostAsync($"{cfg.ServerBaseUrl.TrimEnd('/')}/api/events/ingest", content);
            var body    = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<IngestResponse>(body, _jsonOpts);
                log.ServerEventId = result?.Id;
                log.Domain        = result?.Classification?.Domain;
                log.Category      = result?.Classification?.Category;
                log.Confidence    = result?.Classification?.Confidence;

                await PruneAndSaveLog(log);
                return Json(new { ok = true, id = log.ServerEventId, domain = log.Domain, category = log.Category, confidence = log.Confidence });
            }

            log.ErrorMessage = $"{(int)resp.StatusCode}: {body[..Math.Min(200, body.Length)]}";
            await PruneAndSaveLog(log);
            return Json(new { ok = false, error = log.ErrorMessage });
        }
        catch (Exception ex)
        {
            log.ErrorMessage = ex.Message[..Math.Min(200, ex.Message.Length)];
            await PruneAndSaveLog(log);
            return Json(new { ok = false, error = log.ErrorMessage });
        }
    }

    // ── GET /api/sim-log ──────────────────────────────────────────────────
    [HttpGet("sim-log")]
    public async Task<IActionResult> SimLog()
    {
        var logs = await _db.SimLogs.AsNoTracking()
            .OrderByDescending(l => l.CreatedDate)
            .Take(50)
            .ToListAsync();
        return Json(logs);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task PruneAndSaveLog(SimLog log)
    {
        _db.SimLogs.Add(log);
        await _db.SaveChangesAsync();

        // Keep only 200 most recent
        var count = await _db.SimLogs.CountAsync();
        if (count > 200)
        {
            var oldest = await _db.SimLogs
                .OrderBy(l => l.CreatedDate)
                .Take(count - 200)
                .ToListAsync();
            _db.SimLogs.RemoveRange(oldest);
            await _db.SaveChangesAsync();
        }
    }

    // ── Request / response DTOs ───────────────────────────────────────────

    public sealed class SimulateRequest
    {
        public double  Latitude       { get; set; }
        public double  Longitude      { get; set; }
        public double  SpeedMph       { get; set; }
        public double  AltitudeMeters { get; set; }
        public double  Heading        { get; set; }
        public string? EventType      { get; set; }
        public string? IsAirFlyCar    { get; set; }
    }

    private sealed class IngestResponse
    {
        public bool ok { get; set; }
        public int? Id { get; set; }
        public ClassificationDto? Classification { get; set; }
    }
    private sealed class ClassificationDto
    {
        public string? Domain     { get; set; }
        public string? Category   { get; set; }
        public double? Confidence { get; set; }
    }
}
