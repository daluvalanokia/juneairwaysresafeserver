using EyewaysMergeSafeServer.Data;
using EyewaysMergeSafeServer.Models;
using EyewaysMergeSafeServer.Services;
using EyewaysMergeSafeServer.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EyewaysMergeSafeServer.Controllers;

public class DataInputFormatsController : Controller
{
    private readonly AppDbContext        _db;
    private readonly InputPayloadService _payloadSvc;

    public DataInputFormatsController(AppDbContext db, InputPayloadService payloadSvc)
    { _db = db; _payloadSvc = payloadSvc; }

    public async Task<IActionResult> Index(string activeTab = "physical")
    {
        var highways   = await _db.Highways.AsNoTracking().Where(h => h.IsActive).OrderBy(h => h.Name).ToListAsync();
        var highwayId  = HttpContext.Session.GetString("HighwayId");
        var allConfigs = await _db.InputFormatConfigs.AsNoTracking().OrderBy(c => c.FormatName).ToListAsync();
        var payloads   = await _db.SamplePayloads.AsNoTracking().OrderByDescending(p => p.CreatedDate).Take(30).ToListAsync();

        return View(new DataInputFormatsViewModel
        {
            Highways          = highways,
            SelectedHighwayId = highwayId,
            ActiveTab         = activeTab,
            PhysicalConfigs   = allConfigs.Where(c => c.SourceType == "physical").ToList(),
            SatelliteConfigs  = allConfigs.Where(c => c.SourceType == "satellite").ToList(),
            TelecomConfigs    = allConfigs.Where(c => c.SourceType == "telecom").ToList(),
            TrackerConfigs    = allConfigs.Where(c => c.SourceType == "tracker").ToList(),
            SavedPayloads     = payloads,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InputFormatConfig model, string[] enabledFields, string[]? customFieldNames)
    {
        var combined = enabledFields.ToList();
        if (customFieldNames != null) combined.AddRange(customFieldNames.Where(n => !string.IsNullOrWhiteSpace(n)));
        model.EnabledFieldsRaw = string.Join(",", combined);
        _db.InputFormatConfigs.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { activeTab = model.SourceType });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InputFormatConfig model, string[] enabledFields, string[]? customFieldNames)
    {
        var combined = enabledFields.ToList();
        if (customFieldNames != null) combined.AddRange(customFieldNames.Where(n => !string.IsNullOrWhiteSpace(n)));
        model.EnabledFieldsRaw = string.Join(",", combined);
        _db.InputFormatConfigs.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { activeTab = model.SourceType });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? activeTab)
    {
        var c = await _db.InputFormatConfigs.FindAsync(id);
        if (c != null) { _db.InputFormatConfigs.Remove(c); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index), new { activeTab });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePayload(int configId)
    {
        var config = await _db.InputFormatConfigs.FindAsync(configId);
        if (config == null) return RedirectToAction(nameof(Index));

        var fields  = config.EnabledFieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        var payload = _payloadSvc.Generate(config.SourceType, fields);
        var label   = $"{config.FormatName} — {DateTime.UtcNow:HH:mm:ss}";

        _db.SamplePayloads.Add(new SamplePayload
        {
            ConfigId    = configId,
            SourceType  = config.SourceType,
            Label       = label,
            Payload     = payload,
            IsValid     = true,
            CreatedDate = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { activeTab = config.SourceType });
    }

    /// <summary>AJAX endpoint — returns JSON payload without page reload.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePayloadAjax(int configId)
    {
        var config = await _db.InputFormatConfigs.FindAsync(configId);
        if (config == null)
            return Json(new { ok = false, error = "Config not found" });

        var fields  = config.EnabledFieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        var payload = _payloadSvc.Generate(config.SourceType, fields);
        var label   = $"{config.FormatName} — {DateTime.UtcNow:HH:mm:ss}";

        _db.SamplePayloads.Add(new SamplePayload
        {
            ConfigId    = configId,
            SourceType  = config.SourceType,
            Label       = label,
            Payload     = payload,
            IsValid     = true,
            CreatedDate = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Json(new { ok = true, label, payload });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayload(int id, string? activeTab)
    {
        var p = await _db.SamplePayloads.FindAsync(id);
        if (p != null) { _db.SamplePayloads.Remove(p); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index), new { activeTab });
    }
}
