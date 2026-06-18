using System.Text.Json;
using AirwaysMergeSafeServer.Data;
using AirwaysMergeSafeServer.Models;

namespace AirwaysMergeSafeServer.Services;

public class InputPayloadService
{
    private readonly AppDbContext _db;
    private static readonly Random _rng = new();

    public InputPayloadService(AppDbContext db) { _db = db; }

    public string Generate(string sourceType, IEnumerable<string> enabledFields, IEnumerable<string>? customFields = null)
    {
        var obj = new Dictionary<string, object?>();
        var fields = enabledFields.Concat(customFields ?? Enumerable.Empty<string>());

        foreach (var f in fields)
        {
            obj[f] = f switch
            {
                "vehicle_id"      => $"VEH-{_rng.Next(1000, 9999)}",
                "timestamp"       => DateTime.UtcNow.ToString("o"),
                "speed_mph"       => _rng.Next(20, 100),
                "latitude"        => Math.Round(32.7767 + (_rng.NextDouble() - 0.5) * 0.2, 6),
                "longitude"       => Math.Round(-96.7970 + (_rng.NextDouble() - 0.5) * 0.2, 6),
                "direction"       => _rng.Next(0, 360),
                "lane"            => _rng.Next(1, 5),
                "vehicle_type"    => new[] { "sedan", "suv", "truck", "motorcycle", "van" }[_rng.Next(5)],
                "event_type"      => new[] { "detection", "merge", "speeding", "conflict", "fault" }[_rng.Next(5)],
                "zone_id"         => $"ZONE-{_rng.Next(1, 10):D3}",
                "highway_id"      => "I20-TX",
                "signal_strength" => _rng.Next(-80, -30),
                "altitude_ft"     => _rng.Next(500, 700),
                "heading"         => _rng.Next(0, 360),
                "satellite_count" => _rng.Next(4, 16),
                "hdop"            => Math.Round(_rng.NextDouble() * 2.5, 2),
                "rsrp"            => _rng.Next(-120, -70),
                "rsrq"            => _rng.Next(-15, -3),
                "tag_id"          => $"TAG-{_rng.Next(100000, 999999):X}",
                "read_count"      => _rng.Next(1, 10),
                _                 => $"val_{_rng.Next(100, 999)}"
            };
        }

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<SamplePayload> GenerateAndSaveAsync(int configId)
    {
        var config = await _db.InputFormatConfigs.FindAsync(configId)
            ?? throw new ArgumentException($"Config {configId} not found");

        var fields = config.EnabledFieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();

        var payload = Generate(config.SourceType, fields);

        var sample = new SamplePayload
        {
            ConfigId    = configId,
            SourceType  = config.SourceType,
            Label       = $"{config.FormatName} — {DateTime.UtcNow:HH:mm:ss}",
            Payload     = payload,
            IsValid     = true,
            CreatedDate = DateTime.UtcNow
        };

        _db.SamplePayloads.Add(sample);
        await _db.SaveChangesAsync();
        return sample;
    }
}
