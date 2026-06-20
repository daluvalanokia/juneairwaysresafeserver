using AirwaysMergeSafeServer.Data;
using AirwaysMergeSafeServer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AirwaysMergeSafeServer.Controllers;

/// <summary>
/// Task 10: Fully independent controller for the 3D Air Scene.
/// Serves air-domain vehicles (VehicleMode="air" OR IsAirFlyCar="Y").
/// Completely decoupled from Traffic3DController.
/// Route: /AirScene
/// </summary>
public class AirSceneController : Controller
{
    private readonly AppDbContext  _db;
    private readonly IConfiguration _cfg;

    public AirSceneController(AppDbContext db, IConfiguration cfg)
    { _db = db; _cfg = cfg; }

    [HttpGet]
    public async Task<IActionResult> Index(string? highwayId)
    {
        var highways = await _db.Highways.AsNoTracking()
            .Where(h => h.IsActive).OrderBy(h => h.Name).ToListAsync();
        highwayId ??= HttpContext.Session.GetString("HighwayId")
                      ?? highways.FirstOrDefault()?.HighwayId;
        if (highwayId != null) HttpContext.Session.SetString("HighwayId", highwayId);

        var zones   = await _db.MergeZones.AsNoTracking()
                          .Where(z => z.HighwayId == highwayId).ToListAsync();
        var sensors = await _db.SensorDevices.AsNoTracking()
                          .Where(d => d.HighwayId == highwayId).ToListAsync();
        var zoneIds = zones.Select(z => z.ZoneId).ToList();
        var servers = await _db.SwitchServers.AsNoTracking()
                          .Where(s => s.ZoneId != null && zoneIds.Contains(s.ZoneId))
                          .OrderBy(s => s.ZoneId).ThenBy(s => s.ServerName).ToListAsync();

        // Task 10: air-only query — VehicleMode="air" OR IsAirFlyCar="Y"
        var recentEvents = await _db.VehicleEvents.AsNoTracking()
            .Where(e => e.HighwayId == highwayId
                     && (e.VehicleMode == "air" || e.IsAirFlyCar == "Y"))
            .OrderByDescending(e => e.CreatedDate)
            .Take(120)
            .Select(e => new {
                e.Id, e.VehicleId, e.EventType, e.ZoneId,
                e.SpeedMph, e.Latitude, e.Longitude, e.AltitudeMeters,
                e.VehicleMode, e.VehicleCategory, e.VehicleClassJson,
                e.IsAirFlyCar, e.CreatedDate
            })
            .ToListAsync();

        var groundCount = recentEvents.Count(e => e.VehicleMode == "ground" && e.IsAirFlyCar != "Y");
        var airCount    = recentEvents.Count(e => e.VehicleMode == "air" || e.IsAirFlyCar == "Y");

        var catBreakdown = recentEvents
            .GroupBy(e => e.VehicleCategory)
            .ToDictionary(g => g.Key, g => g.Count());

        return View(new AirSceneViewModel
        {
            Highways           = highways,
            SelectedHighwayId  = highwayId,
            Zones              = zones,
            SwitchServers      = servers,
            Sensors            = sensors,
            RecentEventsJson   = JsonSerializer.Serialize(recentEvents),
            GroundCount        = groundCount,
            AirCount           = airCount,
            CategoryBreakdown  = catBreakdown,
            AirSceneAlertsJson = SettingsController.LoadAirSceneAlertsJson()
        });
    }
}
