namespace AirwaysMergeSafeClient.Services;

public enum ConnectionStatus { NotConfigured, Disconnected, Polling, Connected }

/// <summary>
/// Thread-safe singleton cache holding the latest data polled from the server.
/// </summary>
public class LiveDataCache
{
    private readonly object _lock = new();

    public ConnectionStatus Status     { get; private set; } = ConnectionStatus.NotConfigured;
    public DateTime?        LastPollUtc { get; private set; }
    public string?          LastError   { get; private set; }

    public ServerStatsDto?       Stats            { get; private set; }
    public List<ServerZoneDto>   Zones            { get; private set; } = [];
    public List<ServerEventDto>  LiveEvents       { get; private set; } = [];
    public List<AltitudeBandDto> AltitudeBands    { get; private set; } = [];
    public List<InputFormatDto>  AutomobileFormats { get; private set; } = [];

    // ── Vehicle telemetry (updated each sync loop from ClientConfig) ──────
    public double  VehicleLat      { get; private set; }
    public double  VehicleLon      { get; private set; }
    public double  VehicleSpeedMph { get; private set; }
    public double  VehicleHeading  { get; private set; }
    public double  VehicleAltM     { get; private set; }

    // ── Setters (all thread-safe) ─────────────────────────────────────────

    public void SetPolling()
    {
        lock (_lock) { Status = ConnectionStatus.Polling; }
    }

    public void SetConnected(
        ServerStatsDto?       stats,
        List<ServerZoneDto>   zones,
        List<ServerEventDto>  events,
        List<AltitudeBandDto> bands)
    {
        lock (_lock)
        {
            Status      = ConnectionStatus.Connected;
            LastPollUtc = DateTime.UtcNow;
            LastError   = null;
            Stats       = stats;
            Zones       = zones;
            LiveEvents  = events;
            AltitudeBands = bands;
        }
    }

    public void SetDisconnected(string error)
    {
        lock (_lock)
        {
            Status      = ConnectionStatus.Disconnected;
            LastPollUtc = DateTime.UtcNow;
            LastError   = error;
        }
    }

    public void SetNotConfigured()
    {
        lock (_lock) { Status = ConnectionStatus.NotConfigured; }
    }

    public void SetAutomobileFormats(List<InputFormatDto> formats)
    {
        lock (_lock) { AutomobileFormats = formats; }
    }

    public void SetVehicleTelemetry(double lat, double lon, double speedMph, double heading, double altM)
    {
        lock (_lock)
        {
            VehicleLat      = lat;
            VehicleLon      = lon;
            VehicleSpeedMph = speedMph;
            VehicleHeading  = heading;
            VehicleAltM     = altM;
        }
    }

    /// <summary>Returns a snapshot suitable for JSON serialisation.</summary>
    public object ToSnapshot() => new
    {
        status            = Status.ToString(),
        lastPollUtc       = LastPollUtc,
        lastError         = LastError,
        stats             = Stats,
        zones             = Zones,
        liveEvents        = LiveEvents,
        altitudeBands     = AltitudeBands,
        automobileFormats = AutomobileFormats,
        vehicleLat        = VehicleLat,
        vehicleLon        = VehicleLon,
        vehicleSpeedMph   = VehicleSpeedMph,
        vehicleHeading    = VehicleHeading,
        vehicleAltM       = VehicleAltM
    };
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record ServerStatsDto(int Zones, int Servers, int Sensors, int Events, int Ground, int Air);

public record ServerZoneDto(
    int     Id,
    string  ZoneName,
    string  ZoneId,
    string  HighwayId,
    double? Lat,
    double? Lon,
    double? RadiusM,
    string? ZoneType);

public record ServerEventDto(
    int      Id,
    string   EventType,
    string?  ZoneId,
    string   HighwayId,
    string?  VehicleId,
    double?  SpeedMph,
    double?  Lat,
    double?  Lon,
    double?  AltM,
    string   Mode,
    string   Category,
    string   IsAirFlyCar,
    DateTime CreatedDate);

public record AltitudeBandDto(
    string  ServerId,
    string  ServerName,
    string? ZoneId,
    string  HighwayId,
    double? Min,
    double? Max,
    double? Width);

public record InputFormatDto(
    int     Id,
    string  FormatName,
    string? SourceId,
    string  SourceType,
    string  DataInputType,
    string? InputSource,
    string? Description,
    string? EnabledFieldsRaw);
