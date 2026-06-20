using System.Text;
using System.Text.Json;
using AirwaysMergeSafeClient.Data;
using AirwaysMergeSafeClient.Models;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeClient.Services;

/// <summary>
/// Background service that polls the AirwaysMergeSafeServer on the configured
/// receive interval and optionally sends vehicle events on the send interval.
/// Two independent timers are tracked manually so each can have its own cadence.
/// </summary>
public class ServerSyncService : BackgroundService
{
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly LiveDataCache         _cache;
    private readonly ILogger<ServerSyncService> _logger;
    private readonly IHttpClientFactory    _http;

    private DateTime _lastReceive = DateTime.MinValue;
    private DateTime _lastSend    = DateTime.MinValue;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase
    };

    public ServerSyncService(
        IServiceScopeFactory  scopeFactory,
        LiveDataCache         cache,
        ILogger<ServerSyncService> logger,
        IHttpClientFactory    httpClientFactory)
    {
        _scopeFactory = scopeFactory;
        _cache        = cache;
        _logger       = logger;
        _http         = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Small startup delay so the DB is seeded before first poll
        await Task.Delay(2000, ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                ClientConfig? cfg;
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    cfg = await db.ClientConfig.AsNoTracking().FirstOrDefaultAsync(ct);
                }

                if (cfg is null
                    || string.IsNullOrWhiteSpace(cfg.ServerBaseUrl)
                    || string.IsNullOrWhiteSpace(cfg.DeviceApiKey))
                {
                    _cache.SetNotConfigured();
                    await Task.Delay(5_000, ct);
                    continue;
                }

                var now      = DateTime.UtcNow;
                bool didWork = false;

                if (cfg.ReceiveEnabled
                    && (now - _lastReceive).TotalSeconds >= cfg.ReceivePollSeconds)
                {
                    _cache.SetPolling();
                    await ReceiveAsync(cfg, ct);
                    _lastReceive = DateTime.UtcNow;
                    didWork = true;
                }

                if (cfg.SendEnabled
                    && (now - _lastSend).TotalSeconds >= cfg.SendPollSeconds)
                {
                    await SendPositionAsync(cfg, ct);
                    _lastSend = DateTime.UtcNow;
                    didWork = true;
                }

                await Task.Delay(didWork ? 1_000 : 2_000, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ServerSyncService: unhandled loop error");
                await Task.Delay(5_000, ct);
            }
        }
    }

    // ── Receive ──────────────────────────────────────────────────────────

    private async Task ReceiveAsync(ClientConfig cfg, CancellationToken ct)
    {
        var retries = Math.Max(0, cfg.RetryCount);
        try
        {
            using var http = MakeClient(cfg);
            var base_ = cfg.ServerBaseUrl.TrimEnd('/');
            var hw    = Uri.EscapeDataString(cfg.HighwayId);

            var statsTask   = GetJsonRetry<StatsJson>        ($"{base_}/api/stats?highwayId={hw}",              http, retries, ct);
            var zonesTask   = GetJsonRetry<List<ZoneJson>>   ($"{base_}/api/zones?highwayId={hw}",              http, retries, ct);
            var eventsTask  = GetJsonRetry<List<EventJson>>  ($"{base_}/api/events/live?highwayId={hw}&take={cfg.LiveEventsTake}&since={cfg.LiveEventsSinceMinutes}", http, retries, ct);
            var bandsTask   = GetJsonRetry<List<BandJson>>   ($"{base_}/api/altitudebands?highwayId={hw}",      http, retries, ct);
            var formatsTask = GetJsonRetry<List<FmtJson>>    ($"{base_}/api/input-formats?sourceType=automobile", http, retries, ct);

            await Task.WhenAll(statsTask, zonesTask, eventsTask, bandsTask, formatsTask);

            var stats   = statsTask.Result;
            var zones   = zonesTask.Result   ?? [];
            var events  = eventsTask.Result  ?? [];
            var bands   = bandsTask.Result   ?? [];
            var formats = formatsTask.Result ?? [];

            _cache.SetConnected(
                stats is null ? null : new ServerStatsDto(stats.Zones, stats.Servers, stats.Sensors, stats.Events, stats.Ground, stats.Air),
                zones.Select  (z => new ServerZoneDto(z.Id, z.ZoneName ?? "", z.ZoneId ?? "", z.HighwayId ?? "", z.Lat, z.Lon, z.RadiusM, z.ZoneType)).ToList(),
                events.Select (e => new ServerEventDto(e.Id, e.EventType ?? "detection", e.ZoneId, e.HighwayId ?? "", e.VehicleId, e.SpeedMph, e.Latitude, e.Longitude, e.AltitudeMeters, e.VehicleMode ?? "ground", e.VehicleCategory ?? "sedan", e.IsAirFlyCar ?? "N", e.CreatedDate)).ToList(),
                bands.Select  (b => new AltitudeBandDto(b.ServerId ?? "", b.ServerName ?? "", b.ZoneId, b.HighwayId ?? "", b.AltitudeMinMeters, b.AltitudeMaxMeters, b.AltitudeWidthMeters)).ToList()
            );
            _cache.SetAutomobileFormats(
                formats.Select(f => new InputFormatDto(f.Id, f.FormatName ?? "", f.SourceId, f.SourceType ?? "", f.DataInputType ?? "", f.InputSource, f.Description, f.EnabledFieldsRaw)).ToList()
            );

            _logger.LogInformation("ServerSync: polled — zones={Z} events={E} formats={F}", zones.Count, events.Count, formats.Count);
        }
        catch (Exception ex)
        {
            _cache.SetDisconnected(ex.Message);
            _logger.LogWarning("ServerSync: receive failed — {Err}", ex.Message);
        }
    }

    // ── Send ─────────────────────────────────────────────────────────────

    private async Task SendPositionAsync(ClientConfig cfg, CancellationToken ct)
    {
        var retries = Math.Max(0, cfg.RetryCount);
        try
        {
            using var http = MakeClient(cfg);
            var payload = JsonSerializer.Serialize(new
            {
                eventType      = cfg.DefaultEventType,
                highwayId      = cfg.HighwayId,
                vehicleId      = cfg.VehicleId,
                deviceId       = cfg.DeviceId,
                vehicleType    = cfg.AutoType,
                isAirFlyCar    = cfg.IsAirFlyCar,
                speedMph       = cfg.CurrentSpeedMph,
                latitude       = cfg.CurrentLatitude,
                longitude      = cfg.CurrentLongitude,
                altitudeMeters = cfg.DefaultAltitudeMeters
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage? resp = null;
            for (int attempt = 0; attempt <= retries; attempt++)
            {
                try
                {
                    // Need a fresh content each retry (content is single-use after first read)
                    var c = attempt == 0
                        ? content
                        : new StringContent(payload, Encoding.UTF8, "application/json");
                    resp = await http.PostAsync($"{cfg.ServerBaseUrl.TrimEnd('/')}/api/events/ingest", c, ct);
                    break;
                }
                catch (OperationCanceledException) { throw; }
                catch when (attempt < retries)
                {
                    await Task.Delay(1_000 * (attempt + 1), ct);
                }
            }
            if (resp is not null)
                _logger.LogInformation("ServerSync: sent position — status={S}", resp.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("ServerSync: send failed — {Err}", ex.Message);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private HttpClient MakeClient(ClientConfig cfg)
    {
        var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(Math.Max(5, cfg.HttpTimeoutSeconds));
        client.DefaultRequestHeaders.Add("X-Device-Token", cfg.DeviceApiKey);
        return client;
    }

    /// <summary>GET with up to <paramref name="retries"/> retries (linear 1 s backoff).</summary>
    private async Task<T?> GetJsonRetry<T>(string url, HttpClient client, int retries, CancellationToken ct)
    {
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var resp = await client.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return default;
                var body = await resp.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(body, _json);
            }
            catch (OperationCanceledException) { throw; }
            catch when (attempt < retries)
            {
                await Task.Delay(1_000 * (attempt + 1), ct);
            }
        }
        return default;
    }

    // ── Server JSON shapes ────────────────────────────────────────────────
    private sealed class StatsJson
    {
        public int Zones { get; set; } public int Servers { get; set; } public int Sensors { get; set; }
        public int Events { get; set; } public int Ground { get; set; } public int Air { get; set; }
    }
    private sealed class ZoneJson
    {
        public int Id { get; set; } public string? ZoneName { get; set; } public string? ZoneId { get; set; }
        public string? HighwayId { get; set; } public double? Lat { get; set; } public double? Lon { get; set; }
        public double? RadiusM { get; set; } public string? ZoneType { get; set; }
    }
    private sealed class EventJson
    {
        public int Id { get; set; } public string? EventType { get; set; } public string? ZoneId { get; set; }
        public string? HighwayId { get; set; } public string? VehicleId { get; set; } public double? SpeedMph { get; set; }
        public double? Latitude { get; set; } public double? Longitude { get; set; } public double? AltitudeMeters { get; set; }
        public string? VehicleMode { get; set; } public string? VehicleCategory { get; set; }
        public string? IsAirFlyCar { get; set; } public DateTime CreatedDate { get; set; }
    }
    private sealed class BandJson
    {
        public string? ServerId { get; set; } public string? ServerName { get; set; } public string? ZoneId { get; set; }
        public string? HighwayId { get; set; } public double? AltitudeMinMeters { get; set; }
        public double? AltitudeMaxMeters { get; set; } public double? AltitudeWidthMeters { get; set; }
    }
    private sealed class FmtJson
    {
        public int Id { get; set; } public string? FormatName { get; set; } public string? SourceId { get; set; }
        public string? SourceType { get; set; } public string? DataInputType { get; set; }
        public string? InputSource { get; set; } public string? Description { get; set; }
        public string? EnabledFieldsRaw { get; set; }
    }
}
