using System.Diagnostics;
using AirwaysMergeSafeClient.Data;
using AirwaysMergeSafeClient.Models;
using AirwaysMergeSafeClient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeClient.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext  _db;
    private readonly LiveDataCache _cache;

    public HomeController(AppDbContext db, LiveDataCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var cfg = await _db.ClientConfig.AsNoTracking().FirstOrDefaultAsync()
                  ?? new ClientConfig();
        ViewBag.VehicleName = string.IsNullOrWhiteSpace(cfg.AutoDisplayName)
            ? "Vehicle Client" : cfg.AutoDisplayName;
        ViewBag.Config  = cfg;
        ViewBag.Cache   = _cache;
        ViewBag.Status  = _cache.Status.ToString();
        ViewBag.Snapshot = _cache.ToSnapshot();
        return View(cfg);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
