using AirwaysMergeSafeServer.Models;

namespace AirwaysMergeSafeServer.ViewModels;

public class TriangulationViewModel
{
    public List<Highway> Highways { get; set; } = new();
    public string? SelectedHighwayId { get; set; }
    public List<TriangulationConfig> Configs { get; set; } = new();
}
