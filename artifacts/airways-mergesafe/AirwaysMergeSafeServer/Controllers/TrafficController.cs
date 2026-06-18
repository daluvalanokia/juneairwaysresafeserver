using AirwaysMergeSafeServer.Data;
using AirwaysMergeSafeServer.Services;
using AirwaysMergeSafeServer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeServer.Controllers;

public class TrafficController : Controller
{
    private readonly AppDbContext    _db;
    private readonly TrafficService  _trafficSvc;

    public TrafficController(AppDbContext db, TrafficService trafficSvc)
    { _db = db; _trafficSvc = trafficSvc; }

    public async Task<IActionResult> Index(string? highwayId)
    {
        var highways = await _db.Highways.AsNoTracking().Where(h => h.IsActive).OrderBy(h => h.Name).ToListAsync();
        highwayId ??= HttpContext.Session.GetString("HighwayId") ?? highways.FirstOrDefault()?.HighwayId;
        if (highwayId != null) HttpContext.Session.SetString("HighwayId", highwayId);

        var zones   = await _db.MergeZones.AsNoTracking().Where(z => z.HighwayId == highwayId).ToListAsync();
        var sensors = await _db.SensorDevices.AsNoTracking().Where(d => d.HighwayId == highwayId).ToListAsync();

        return View(new Traffic3DViewModel
        {
            Highways          = highways,
            SelectedHighwayId = highwayId,
            Zones             = zones,
            Sensors           = sensors,
            TomTomApiKey      = HttpContext.RequestServices.GetService<IConfiguration>()?["TomTomApiKey"]
        });
    }

    [HttpGet, OutputCache(PolicyName = "ShortLive")]
    public async Task<IActionResult> GetSegments(string highwayId)
    {
        var result = await _trafficSvc.GetSegmentsAsync(highwayId);
        return Json(result);
    }
}
