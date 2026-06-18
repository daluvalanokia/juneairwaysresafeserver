using AirwaysMergeSafeServer.Data;
using AirwaysMergeSafeServer.Models;
using AirwaysMergeSafeServer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirwaysMergeSafeServer.Controllers;

public class SwitchServersController : Controller
{
    private readonly AppDbContext _db;
    public SwitchServersController(AppDbContext db) { _db = db; }

    private bool IsAjax => Request.Headers["X-Requested-With"] == "XMLHttpRequest";

    public async Task<IActionResult> Index(string? highwayId)
    {
        var highways = await _db.Highways.AsNoTracking().Where(h => h.IsActive).OrderBy(h => h.Name).ToListAsync();
        highwayId ??= HttpContext.Session.GetString("HighwayId") ?? highways.FirstOrDefault()?.HighwayId;
        if (highwayId != null) HttpContext.Session.SetString("HighwayId", highwayId);

        var servers = await _db.SwitchServers.AsNoTracking()
            .Where(s => s.HighwayId == highwayId)
            .OrderBy(s => s.ZoneId).ThenBy(s => s.ServerName)
            .ToListAsync();

        return View(new SwitchServerViewModel { Highways = highways, SelectedHighwayId = highwayId, Servers = servers });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SwitchServer model)
    {
        _db.SwitchServers.Add(model);
        await _db.SaveChangesAsync();
        if (IsAjax) return Json(new { ok = true, highwayId = model.HighwayId });
        return RedirectToAction(nameof(Index), new { highwayId = model.HighwayId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SwitchServer model)
    {
        _db.SwitchServers.Update(model);
        await _db.SaveChangesAsync();
        if (IsAjax) return Json(new { ok = true, highwayId = model.HighwayId });
        return RedirectToAction(nameof(Index), new { highwayId = model.HighwayId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? highwayId)
    {
        var s = await _db.SwitchServers.FindAsync(id);
        if (s != null) { _db.SwitchServers.Remove(s); await _db.SaveChangesAsync(); }
        if (IsAjax) return Json(new { ok = true });
        return RedirectToAction(nameof(Index), new { highwayId });
    }
}
