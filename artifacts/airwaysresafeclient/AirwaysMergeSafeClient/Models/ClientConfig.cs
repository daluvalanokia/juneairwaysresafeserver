using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirwaysMergeSafeClient.Models;

[Table("ClientConfig")]
public class ClientConfig
{
    [Key] public int Id { get; set; }

    // ── User profile ─────────────────────────────────────────────────────
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(30)]  public string Phone    { get; set; } = "";
    [MaxLength(200)] public string Address  { get; set; } = "";

    // ── Automobile ───────────────────────────────────────────────────────
    [MaxLength(60)] public string AutoDisplayName { get; set; } = "My Vehicle";
    [MaxLength(50)] public string AutoMake        { get; set; } = "";
    [MaxLength(50)] public string AutoModel       { get; set; } = "";
    public int AutoYear { get; set; } = DateTime.UtcNow.Year;
    /// <summary>sedan|suv|truck|motorcycle|van|air_urban|air_express</summary>
    [MaxLength(30)] public string AutoType   { get; set; } = "sedan";
    /// <summary>"Y" or "N"</summary>
    [MaxLength(1)]  public string IsAirFlyCar { get; set; } = "N";

    // ── Server connection ────────────────────────────────────────────────
    [MaxLength(200)] public string ServerBaseUrl { get; set; } = "";
    /// <summary>Sent as X-Device-Token header on every server API call.</summary>
    [MaxLength(100)] public string DeviceApiKey { get; set; } = "";
    [MaxLength(50)]  public string HighwayId    { get; set; } = "";
    [MaxLength(60)]  public string VehicleId    { get; set; } = "";
    [MaxLength(60)]  public string DeviceId     { get; set; } = "";
    public bool AutoConnectOnStartup { get; set; } = false;

    // ── Receive settings ─────────────────────────────────────────────────
    public bool ReceiveEnabled         { get; set; } = false;
    public int  ReceivePollSeconds     { get; set; } = 10;
    public int  LiveEventsTake         { get; set; } = 50;
    public int  LiveEventsSinceMinutes { get; set; } = 5;

    // ── Send settings ────────────────────────────────────────────────────
    public bool SendEnabled { get; set; } = false;
    public int  SendPollSeconds { get; set; } = 5;
    [MaxLength(20)] public string DefaultEventType     { get; set; } = "detection";
    public double DefaultAltitudeMeters { get; set; } = 0;

    // ── Current position (updated by simulation / manual GPS entry) ──────
    public double CurrentLatitude  { get; set; } = 32.7767;
    public double CurrentLongitude { get; set; } = -96.7970;
    public double CurrentSpeedMph  { get; set; } = 0;
    public double CurrentHeading   { get; set; } = 0;

    // ── Reliability ──────────────────────────────────────────────────────
    public int HttpTimeoutSeconds { get; set; } = 15;
    public int RetryCount         { get; set; } = 2;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}
