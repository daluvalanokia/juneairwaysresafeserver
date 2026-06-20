using Microsoft.EntityFrameworkCore;
using AirwaysMergeSafeClient.Models;

namespace AirwaysMergeSafeClient.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ClientConfig> ClientConfig => Set<ClientConfig>();
    public DbSet<SimLog>       SimLogs      => Set<SimLog>();

    /// <summary>Seed a single default ClientConfig row if none exists.</summary>
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ClientConfig.AnyAsync()) return;

        var vehicleId = $"VH-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        var deviceId  = $"DEV-{Guid.NewGuid():N}"[..12].ToUpperInvariant();

        db.ClientConfig.Add(new ClientConfig
        {
            FullName           = "",
            Phone              = "",
            Address            = "",
            AutoDisplayName    = "My Vehicle",
            AutoMake           = "",
            AutoModel          = "",
            AutoYear           = DateTime.UtcNow.Year,
            AutoType           = "sedan",
            IsAirFlyCar        = "N",
            ServerBaseUrl      = "",
            DeviceApiKey       = "",
            HighwayId          = "",
            VehicleId          = vehicleId,
            DeviceId           = deviceId,
            AutoConnectOnStartup = false,
            ReceiveEnabled     = false,
            ReceivePollSeconds = 10,
            LiveEventsTake     = 50,
            LiveEventsSinceMinutes = 5,
            SendEnabled        = false,
            SendPollSeconds    = 5,
            DefaultEventType   = "detection",
            DefaultAltitudeMeters = 0,
            CurrentLatitude    = 32.7767,
            CurrentLongitude   = -96.7970,
            HttpTimeoutSeconds = 15,
            RetryCount         = 2,
            UpdatedDate        = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
