using System.Text.Json;
using AirwaysMergeSafeServer.Data;
using AirwaysMergeSafeServer.Infrastructure;
using AirwaysMergeSafeServer.Models;

namespace AirwaysMergeSafeServer.Services;

/// <summary>
/// Phase 8: Added "airflycar" source type generator.
///
/// AirFlyCar payload fields generated:
///   Core 4D position: latitude, longitude, altitude_m, heading, speed_mph, vertical_rate_fpm
///   Flight state:     vehicle_type (air_urban|air_express), flight_phase, corridor_id
///   UAM telemetry:    battery_soc, battery_temp_c, rotor_rpm, rotor_health, motor_temp_c
///   Safety fields:    conflict_flag, separation_m, corridor_deviation_m, noise_db
///   Fleet:            passenger_count, destination_pad, pilot_id, range_remaining_km
///   Identity:         vehicle_id (AFC-XXXX format), icao_address, squawk, timestamp
///
/// Altitude generated in two realistic bands:
///   air_urban   — 30–149 m  (urban air mobility corridor)
///   air_express — 151–800 m (express / inter-city corridor)
///   air_vertiport — 0–3 m   (on-ground at vertiport pad)
///
/// All other source types (physical/satellite/telecom/tracker) preserved exactly.
/// Random.Shared used throughout (A1 fix maintained).
/// </summary>
public class InputPayloadService
{
    private readonly AppDbContext _db;
    public InputPayloadService(AppDbContext db) { _db = db; }

    private static readonly string[] GroundTypes      = { "sedan", "suv", "truck", "motorcycle", "van" };
    private static readonly string[] AirTypes         = { "air_urban", "air_express" };
    private static readonly string[] FlightPhases     = { "climb", "cruise", "descent", "hover", "approach" };
    private static readonly string[] GroundPhases     = { "boarding", "deboarding", "charging", "ground" };
    private static readonly string[] Corridors        = { "COR-DFW-N1", "COR-DFW-S2", "COR-AUS-E1", "COR-HOU-W3", "COR-SAT-C1" };
    private static readonly string[] DestPads         = { "PAD-DFW-01", "PAD-DFW-02", "PAD-AUS-01", "PAD-HOU-03", "PAD-SAT-01" };
    private static readonly string[] RotorHealthStates= { "nominal", "nominal", "nominal", "degraded", "warning" };

    public string Generate(
        string               sourceType,
        IEnumerable<string>  enabledFields,
        IEnumerable<string>? customFields = null)
    {
        TraceLogger.Enter("InputPayloadService", nameof(Generate), $"sourceType={sourceType}");
        try
        {
        var rng    = Random.Shared;
        var obj    = new Dictionary<string, object?>();
        var fields = enabledFields.Concat(customFields ?? Enumerable.Empty<string>()).Distinct();

        if (string.Equals(sourceType, "airflycar", StringComparison.OrdinalIgnoreCase))
        {
            var _afc = GenerateAirFlyCar(rng, fields);
            TraceLogger.Exit("InputPayloadService", nameof(Generate), "airflycar");
            return _afc;
        }

        if (string.Equals(sourceType, "automobile", StringComparison.OrdinalIgnoreCase))
        {
            var _auto = GenerateAutomobile(rng, fields);
            TraceLogger.Exit("InputPayloadService", nameof(Generate), "automobile");
            return _auto;
        }

        // ── Existing source types (preserved) ────────────────────────────
        bool isAirSource  = sourceType is "satellite" or "tracker";
        bool isAirVehicle = isAirSource && rng.NextDouble() < 0.30;
        string vehicleType = isAirVehicle
            ? AirTypes[rng.Next(AirTypes.Length)]
            : GroundTypes[rng.Next(GroundTypes.Length)];

        double altitudeM = isAirVehicle
            ? (vehicleType == "air_urban" ? rng.Next(30, 150) : rng.Next(151, 801))
            : Math.Round(rng.NextDouble() * 5, 1);

        foreach (var f in fields)
        {
            obj[f] = f switch
            {
                "vehicle_id"      => $"VEH-{rng.Next(1000, 9999)}",
                "timestamp"       => DateTime.UtcNow.ToString("o"),
                "speed_mph"       => isAirVehicle ? rng.Next(80, 180) : rng.Next(20, 100),
                "latitude"        => Math.Round(32.7767 + (rng.NextDouble() - 0.5) * 0.2, 6),
                "longitude"       => Math.Round(-96.7970 + (rng.NextDouble() - 0.5) * 0.2, 6),
                "altitude_m"      => altitudeM,
                "altitude_ft"     => Math.Round(altitudeM * 3.28084, 1),
                "vehicle_type"    => vehicleType,
                "direction"       => rng.Next(0, 360),
                "lane"            => isAirVehicle ? rng.Next(10, 20) : rng.Next(1, 5),
                "event_type"      => new[] { "detection","merge","speeding","conflict","fault" }[rng.Next(5)],
                "zone_id"         => $"ZONE-{rng.Next(1, 10):D3}",
                "highway_id"      => "I20-TX",
                "signal_strength" => sourceType == "telecom" ? rng.Next(-80, -30) : rng.Next(-95, -40),
                "heading"         => rng.Next(0, 360),
                "satellite_count" => rng.Next(4, 16),
                "hdop"            => Math.Round(rng.NextDouble() * 2.5, 2),
                "rsrp"            => rng.Next(-120, -70),
                "rsrq"            => rng.Next(-15, -3),
                "tag_id"          => $"TAG-{rng.Next(100000, 999999):X}",
                "read_count"      => rng.Next(1, 10),
                "isAirFlyCar"     => "N",   // always "N" for non-airflycar sources
                _                 => $"val_{rng.Next(100, 999)}"
            };
        }

        var _json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        TraceLogger.Exit("InputPayloadService", nameof(Generate), $"sourceType={sourceType}");
        return _json;
        }
        catch (Exception ex) { TraceLogger.Error("InputPayloadService", nameof(Generate), ex); throw; }
    }

    /// <summary>
    /// Phase 8: AirFlyCar payload generator.
    /// Produces realistic UAM telemetry with correlated fields
    /// (flight_phase drives altitude range, battery_soc drives range_remaining_km, etc.)
    /// </summary>
    private static string GenerateAirFlyCar(Random rng, IEnumerable<string> fields)
    {
        var obj = new Dictionary<string, object?>();

        // ── Determine flight state first — all other fields follow ────────
        bool isGrounded   = rng.NextDouble() < 0.15;          // 15% on vertiport pad
        string flightPhase = isGrounded
            ? GroundPhases[rng.Next(GroundPhases.Length)]
            : FlightPhases[rng.Next(FlightPhases.Length)];

        // Altitude band
        double altM;
        string vehicleType;
        if (isGrounded)
        {
            altM        = Math.Round(rng.NextDouble() * 3, 1); // 0–3 m on pad
            vehicleType = "air_urban";  // on-pad craft are urban-class
        }
        else
        {
            bool isExpress = rng.NextDouble() < 0.35; // 35% express corridor
            altM        = isExpress ? rng.Next(151, 801) : rng.Next(30, 150);
            vehicleType = isExpress ? "air_express" : "air_urban";
        }

        // Correlated kinematics
        double speedMph = isGrounded ? 0 : flightPhase switch
        {
            "climb"    => rng.Next(40, 120),
            "cruise"   => rng.Next(100, 180),
            "descent"  => rng.Next(30, 100),
            "hover"    => rng.Next(0,  15),
            "approach" => rng.Next(20, 70),
            _          => rng.Next(60, 150)
        };

        double vertRateFpm = isGrounded ? 0 : flightPhase switch
        {
            "climb"    =>  rng.Next(300, 1200),
            "cruise"   =>  rng.Next(-50, 50),
            "descent"  => -rng.Next(200, 800),
            "hover"    =>  rng.Next(-20, 20),
            "approach" => -rng.Next(50, 400),
            _          =>  0
        };

        // Battery — lower SOC on longer express corridors
        double battSoc      = vehicleType == "air_express"
            ? Math.Round(30 + rng.NextDouble() * 50, 1)   // 30–80%
            : Math.Round(50 + rng.NextDouble() * 48, 1);  // 50–98%

        double rangeKm      = Math.Round(battSoc * 0.8 + rng.NextDouble() * 20, 1);
        double rotorRpm     = isGrounded ? rng.Next(0, 200) : rng.Next(1800, 3200);
        double motorTempC   = isGrounded ? rng.Next(20, 40) : rng.Next(55, 110);
        double battTempC    = isGrounded ? rng.Next(20, 35) : rng.Next(30, 55);
        double noiseDb      = isGrounded ? rng.Next(40, 65) : rng.Next(60, 85);
        bool   conflictFlag = !isGrounded && rng.NextDouble() < 0.08; // 8% conflict chance
        double separationM  = conflictFlag ? rng.Next(50, 300) : rng.Next(400, 2000);
        double corrDevM     = isGrounded   ? 0 : Math.Round(rng.NextDouble() * 80, 1);
        string corridorId   = Corridors[rng.Next(Corridors.Length)];
        string icao         = $"{rng.Next(0x400000, 0xFFFFFF):X6}";
        int    squawk       = rng.Next(1000, 7776);
        string pilotId      = $"PLT-{rng.Next(100, 999)}";
        string destPad      = DestPads[rng.Next(DestPads.Length)];
        string rotorHealth  = RotorHealthStates[rng.Next(RotorHealthStates.Length)];

        // Lat/lon centred on DFW area, spread across corridors
        double lat = Math.Round(32.7767 + (rng.NextDouble() - 0.5) * 0.4, 6);
        double lon = Math.Round(-96.7970 + (rng.NextDouble() - 0.5) * 0.4, 6);

        string eventType = conflictFlag ? "conflict"
            : flightPhase == "approach" ? "merge"
            : "detection";

        // ── Map requested fields to generated values ──────────────────────
        foreach (var f in fields)
        {
            obj[f] = f switch
            {
                "vehicle_id"           => $"AFC-{rng.Next(1000, 9999)}",
                "timestamp"            => DateTime.UtcNow.ToString("o"),
                "latitude"             => lat,
                "longitude"            => lon,
                "altitude_m"           => altM,
                "speed_mph"            => Math.Round(speedMph, 1),
                "heading"              => rng.Next(0, 360),
                "vehicle_type"         => vehicleType,
                "flight_phase"         => flightPhase,
                "vertical_rate_fpm"    => vertRateFpm,
                "battery_soc"          => battSoc,
                "battery_temp_c"       => battTempC,
                "range_remaining_km"   => rangeKm,
                "rotor_rpm"            => rotorRpm,
                "rotor_health"         => rotorHealth,
                "motor_temp_c"         => motorTempC,
                "noise_db"             => noiseDb,
                "corridor_id"          => corridorId,
                "corridor_deviation_m" => corrDevM,
                "conflict_flag"        => conflictFlag ? 1 : 0,
                "separation_m"         => Math.Round(separationM, 1),
                "passenger_count"      => isGrounded ? rng.Next(0, 5) : rng.Next(1, 5),
                "destination_pad"      => destPad,
                "pilot_id"             => pilotId,
                "icao_address"         => icao,
                "squawk"               => squawk,
                "nic"                  => rng.Next(8, 12),       // ADS-B navigation integrity
                "nac_p"                => rng.Next(8, 11),       // navigation accuracy
                "zone_id"              => $"ZONE-{rng.Next(1, 10):D3}",
                "highway_id"           => "I20-TX",
                "event_type"           => eventType,
                "isAirFlyCar"          => "Y",   // always "Y" — this IS an AirFlyCar source
                _                      => $"val_{rng.Next(100, 999)}"
            };
        }

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Automobile / Connected-Vehicle payload generator.
    /// Produces GPS + OBD-II telemetry for a ground vehicle, similar in structure
    /// to the Satellite/GPS type but with car-specific fields.
    /// </summary>
    private static string GenerateAutomobile(Random rng, IEnumerable<string> fields)
    {
        var obj = new Dictionary<string, object?>();

        // Ground vehicle — always near-zero altitude
        double altM     = Math.Round(rng.NextDouble() * 3, 1);
        string vType    = GroundTypes[rng.Next(GroundTypes.Length)];
        double speedMph = rng.Next(0, 85);
        double lat      = Math.Round(32.7767 + (rng.NextDouble() - 0.5) * 0.3, 6);
        double lon      = Math.Round(-96.7970 + (rng.NextDouble() - 0.5) * 0.3, 6);

        // OBD-II / connected-vehicle state
        string[] makes   = { "Toyota", "Ford", "Chevrolet", "Honda", "Tesla", "BMW", "Hyundai" };
        string[] models  = { "Camry", "F-150", "Silverado", "Accord", "Model 3", "3-Series", "Elantra" };
        string[] gears   = { "P", "R", "N", "1", "2", "3", "4", "5", "6", "D" };
        string   makeVal = makes[rng.Next(makes.Length)];
        string   modelVal= models[rng.Next(models.Length)];
        int      yearVal = rng.Next(2018, 2026);
        double   odo     = Math.Round(rng.Next(0, 180000) + rng.NextDouble(), 1);
        double   engTemp = Math.Round(70 + rng.NextDouble() * 35, 1);   // 70–105 °C normal
        double   fuel    = Math.Round(5 + rng.NextDouble() * 95, 1);    // 5–100 %
        int      rpm     = speedMph < 5 ? rng.Next(700, 900) : rng.Next(1000, 5500);
        string   gear    = speedMph < 2 ? "P" : gears[rng.Next(2, gears.Length)];
        double   throttle= Math.Round(rng.NextDouble() * 80, 1);
        double   brake   = speedMph < 5 ? Math.Round(rng.NextDouble() * 40, 1) : 0;
        double   battV   = Math.Round(12.0 + rng.NextDouble() * 2.8, 2);   // 12.0–14.8 V
        int      absAct  = rng.NextDouble() < 0.05 ? 1 : 0;
        int      tcAct   = rng.NextDouble() < 0.04 ? 1 : 0;
        string   obd     = rng.NextDouble() < 0.07
                            ? $"P{rng.Next(100, 3000):D4}"   // 7 % chance of fault code
                            : "P0000";
        double   tpFl    = Math.Round(30 + rng.NextDouble() * 6, 1);
        double   tpFr    = Math.Round(30 + rng.NextDouble() * 6, 1);
        double   tpRl    = Math.Round(30 + rng.NextDouble() * 6, 1);
        double   tpRr    = Math.Round(30 + rng.NextDouble() * 6, 1);

        foreach (var f in fields)
        {
            obj[f] = f switch
            {
                "vehicle_id"       => $"AUTO-{rng.Next(1000, 9999)}",
                "timestamp"        => DateTime.UtcNow.ToString("o"),
                "latitude"         => lat,
                "longitude"        => lon,
                "altitude_m"       => altM,
                "altitude_ft"      => Math.Round(altM * 3.28084, 1),
                "speed_mph"        => Math.Round(speedMph, 1),
                "heading"          => rng.Next(0, 360),
                "direction"        => rng.Next(0, 360),
                "lane"             => rng.Next(1, 5),
                "vehicle_type"     => vType,
                "event_type"       => new[] { "detection","merge","speeding" }[rng.Next(3)],
                "zone_id"          => $"ZONE-{rng.Next(1, 10):D3}",
                "highway_id"       => "I20-TX",
                "satellite_count"  => rng.Next(4, 16),
                "hdop"             => Math.Round(rng.NextDouble() * 2.5, 2),
                "isAirFlyCar"      => "N",
                // Automobile-specific OBD-II / connected-vehicle fields
                "vin"              => $"1HGBH41JXMN{rng.Next(100000, 999999)}",
                "make"             => makeVal,
                "model"            => modelVal,
                "year"             => yearVal,
                "odometer_km"      => odo,
                "engine_temp_c"    => engTemp,
                "fuel_level_pct"   => fuel,
                "rpm"              => rpm,
                "gear"             => gear,
                "throttle_pct"     => throttle,
                "brake_pct"        => brake,
                "battery_voltage"  => battV,
                "abs_active"       => absAct,
                "traction_control" => tcAct,
                "obd_code"         => obd,
                "tire_pressure_fl" => tpFl,
                "tire_pressure_fr" => tpFr,
                "tire_pressure_rl" => tpRl,
                "tire_pressure_rr" => tpRr,
                _                  => $"val_{rng.Next(100, 999)}"
            };
        }

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Parses altitude from a raw JSON payload string.
    /// Checks fields: altitude_m, alt_m, alt, altitude, elevation (in that order).
    /// Returns null if none found or payload is not valid JSON.
    /// </summary>
    public static double? ParseAltitude(string payloadJson)
    {
        TraceLogger.Enter("InputPayloadService", nameof(ParseAltitude));
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            TraceLogger.Exit("InputPayloadService", nameof(ParseAltitude), "null-input");
            return null;
        }
        try
        {
            using var doc  = JsonDocument.Parse(payloadJson);
            var root       = doc.RootElement;
            var candidates = new[] { "altitude_m", "alt_m", "alt", "altitude", "elevation" };
            foreach (var key in candidates)
                if (root.TryGetProperty(key, out var val) && val.TryGetDouble(out double d))
                {
                    TraceLogger.Exit("InputPayloadService", nameof(ParseAltitude), $"{d}m via '{key}'");
                    return d;
                }
        }
        catch (Exception ex) { TraceLogger.Error("InputPayloadService", nameof(ParseAltitude), ex); }
        TraceLogger.Exit("InputPayloadService", nameof(ParseAltitude), "not-found");
        return null;
    }

    public async Task<SamplePayload> GenerateAndSaveAsync(int configId)
    {
        TraceLogger.Enter("InputPayloadService", nameof(GenerateAndSaveAsync), $"configId={configId}");
        try
        {
            var config = await _db.InputFormatConfigs.FindAsync(configId)
                ?? throw new ArgumentException($"Config {configId} not found");
            var fields  = config.EnabledFieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          ?? Array.Empty<string>();
            var payload = Generate(config.SourceType, fields);
            var sample  = new SamplePayload
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
            TraceLogger.Exit("InputPayloadService", nameof(GenerateAndSaveAsync), $"sampleId={sample.Id}");
            return sample;
        }
        catch (Exception ex) { TraceLogger.Error("InputPayloadService", nameof(GenerateAndSaveAsync), ex); throw; }
    }
}
