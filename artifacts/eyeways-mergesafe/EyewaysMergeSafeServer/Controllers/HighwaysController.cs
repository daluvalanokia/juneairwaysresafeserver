using EyewaysMergeSafeServer.Data;
using EyewaysMergeSafeServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace EyewaysMergeSafeServer.Controllers;

public class HighwaysController : Controller
{
    private readonly AppDbContext _db;
    public HighwaysController(AppDbContext db) { _db = db; }

    [OutputCache(PolicyName = "Highways")]
    public async Task<IActionResult> Index()
    {
        var highways = await _db.Highways.AsNoTracking().OrderBy(h => h.Name).ToListAsync();
        return View(highways);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,HighwayId,State,Description,IsActive")] Highway model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        model.CreatedDate = DateTime.UtcNow;
        _db.Highways.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("Id,Name,HighwayId,State,Description,IsActive,CreatedDate")] Highway model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        _db.Highways.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var h = await _db.Highways.FindAsync(id);
        if (h != null) { _db.Highways.Remove(h); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var highways = await _db.Highways.AsNoTracking()
            .Where(h => h.IsActive).OrderBy(h => h.Name)
            .Select(h => new { h.Id, h.Name, h.HighwayId, h.State, h.IsActive })
            .ToListAsync();
        return Json(highways);
    }
}
