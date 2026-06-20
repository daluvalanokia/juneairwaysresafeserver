using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirwaysMergeSafeClient.Models;

[Table("SimLogs")]
public class SimLog
{
    [Key] public int Id { get; set; }
    [MaxLength(60)] public string  VehicleId    { get; set; } = "";
    [MaxLength(30)] public string  EventType    { get; set; } = "detection";
    [MaxLength(50)] public string  HighwayId    { get; set; } = "";
    public double Latitude       { get; set; }
    public double Longitude      { get; set; }
    public double AltitudeMeters { get; set; }
    public double SpeedMph       { get; set; }
    [MaxLength(1)]  public string  IsAirFlyCar  { get; set; } = "N";
    public int?    ServerEventId { get; set; }
    [MaxLength(20)] public string? Domain       { get; set; }
    [MaxLength(30)] public string? Category     { get; set; }
    public double? Confidence    { get; set; }
    [MaxLength(300)] public string? ErrorMessage { get; set; }
    public DateTime CreatedDate  { get; set; } = DateTime.UtcNow;
}
