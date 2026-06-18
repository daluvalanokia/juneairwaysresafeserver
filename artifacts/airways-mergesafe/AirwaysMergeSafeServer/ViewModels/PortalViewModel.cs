using AirwaysMergeSafeServer.Models;

namespace AirwaysMergeSafeServer.ViewModels;

public class PortalViewModel
{
    public List<Highway> Highways { get; set; } = new();
    public string? SelectedHighwayId { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public string? Error { get; set; }
}
