using AirwaysMergeSafeServer.Models;

namespace AirwaysMergeSafeServer.ViewModels;

public class SwitchServerViewModel
{
    public List<Highway> Highways { get; set; } = new();
    public string? SelectedHighwayId { get; set; }
    public List<SwitchServer> Servers { get; set; } = new();
    /// <summary>Aggregate min altitude band across selected servers (display).</summary>
    public double? AltitudeMinMeters   => Servers.Where(s => s.AltitudeMinMeters.HasValue).Select(s => s.AltitudeMinMeters).Min();
    /// <summary>Aggregate max altitude band across selected servers (display).</summary>
    public double? AltitudeMaxMeters   => Servers.Where(s => s.AltitudeMaxMeters.HasValue).Select(s => s.AltitudeMaxMeters).Max();
    /// <summary>Average altitude width across selected servers (display).</summary>
    public double? AltitudeWidthMeters => Servers.Where(s => s.AltitudeWidthMeters.HasValue).Select(s => s.AltitudeWidthMeters).Average();
}
