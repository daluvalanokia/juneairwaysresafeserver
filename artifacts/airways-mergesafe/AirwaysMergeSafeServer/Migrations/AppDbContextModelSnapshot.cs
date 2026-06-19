using System;
using AirwaysMergeSafeServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AirwaysMergeSafeServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.AuditLog", b =>
            {
                b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<string>("Action").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.Property<string>("Controller").IsRequired().HasMaxLength(60).HasColumnType("TEXT");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("EntityId").HasMaxLength(80).HasColumnType("TEXT");
                b.Property<string>("EntityType").HasMaxLength(60).HasColumnType("TEXT");
                b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("HighwayId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("IpAddress").HasMaxLength(45).HasColumnType("TEXT");
                b.Property<string>("Summary").HasMaxLength(500).HasColumnType("TEXT");
                b.Property<string>("UserId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("AuditLogs");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.Highway", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("Description").HasMaxLength(300).HasColumnType("TEXT");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<bool>("IsActive").HasColumnType("INTEGER");
                b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("State").HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Highways");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.InputFormatConfig", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("Description").HasMaxLength(300).HasColumnType("TEXT");
                b.Property<string>("EnabledFieldsRaw").HasMaxLength(1000).HasColumnType("TEXT");
                b.Property<string>("FormatName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("InputSource").HasMaxLength(300).HasColumnType("TEXT");
                b.Property<string>("SourceId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("SourceType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("SourceType");
                b.ToTable("InputFormatConfigs");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.MergeZone", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<double?>("AltitudeMeters").HasColumnType("REAL");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<int>("GeofenceRadius").HasColumnType("INTEGER");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Latitude").HasColumnType("REAL");
                b.Property<double?>("Longitude").HasColumnType("REAL");
                b.Property<double?>("MileMarker").HasColumnType("REAL");
                b.Property<string>("Status").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.Property<string>("ZoneId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("ZoneName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("HighwayId");
                b.ToTable("MergeZones");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.SamplePayload", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<int?>("ConfigId").HasColumnType("INTEGER");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<bool>("IsValid").HasColumnType("INTEGER");
                b.Property<string>("Label").HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("Payload").HasColumnType("TEXT");
                b.Property<string>("SourceType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("SamplePayloads");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.SensorDevice", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<double?>("AltitudeMeters").HasColumnType("REAL");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("DeviceId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("DeviceName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("DeviceType").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("FirmwareVersion").HasMaxLength(20).HasColumnType("TEXT");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<DateTime>("LastHeartbeat").HasColumnType("TEXT");
                b.Property<double?>("Latitude").HasColumnType("REAL");
                b.Property<double?>("Longitude").HasColumnType("REAL");
                b.Property<double?>("MileMarker").HasColumnType("REAL");
                b.Property<string>("Status").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.Property<string>("ZoneId").HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("HighwayId");
                b.HasIndex("ZoneId");
                b.ToTable("SensorDevices");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.SwitchServer", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<double?>("AltitudeMaxMeters").HasColumnType("REAL");
                b.Property<double?>("AltitudeMinMeters").HasColumnType("REAL");
                b.Property<double?>("AltitudeWidthMeters").HasColumnType("REAL");
                b.Property<double>("CpuPercent").HasColumnType("REAL");
                b.Property<string>("GpsLocation").HasMaxLength(60).HasColumnType("TEXT");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("FirmwareVersion").HasMaxLength(20).HasColumnType("TEXT");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("IpAddress").HasMaxLength(45).HasColumnType("TEXT");
                b.Property<DateTime>("LastHeartbeat").HasColumnType("TEXT");
                b.Property<double>("MemoryPercent").HasColumnType("REAL");
                b.Property<int>("Port").HasColumnType("INTEGER");
                b.Property<string>("ServerId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("ServerName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("Status").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.Property<long>("UptimeSeconds").HasColumnType("INTEGER");
                b.Property<string>("ZoneId").HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("HighwayId");
                b.HasIndex("ZoneId");
                b.ToTable("SwitchServers");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.TriangulationConfig", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<int>("GeofenceRadius").HasColumnType("INTEGER");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<bool>("IsActive").HasColumnType("INTEGER");
                b.Property<double?>("Switch1Lat").HasColumnType("REAL");
                b.Property<string>("Switch1Label").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Switch1Lon").HasColumnType("REAL");
                b.Property<string>("Switch1ServerId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Switch2Lat").HasColumnType("REAL");
                b.Property<string>("Switch2Label").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Switch2Lon").HasColumnType("REAL");
                b.Property<string>("Switch2ServerId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Switch3Lat").HasColumnType("REAL");
                b.Property<string>("Switch3Label").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Switch3Lon").HasColumnType("REAL");
                b.Property<string>("Switch3ServerId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("ZoneId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("TriangulationConfigs");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.UserProfile", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<string>("Address").HasMaxLength(200).HasColumnType("TEXT");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("DeviceIdsRaw").HasMaxLength(500).HasColumnType("TEXT");
                b.Property<int>("FailedLoginAttempts").HasColumnType("INTEGER");
                b.Property<string>("FullName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                b.Property<string>("HighwayId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("HighwayName").HasMaxLength(100).HasColumnType("TEXT");
                b.Property<bool>("IsActive").HasColumnType("INTEGER");
                b.Property<DateTime?>("LockedUntil").HasColumnType("TEXT");
                b.Property<string>("Notes").HasMaxLength(300).HasColumnType("TEXT");
                b.Property<string>("Password").HasMaxLength(200).HasColumnType("TEXT");
                b.Property<string>("Phone").HasMaxLength(20).HasColumnType("TEXT");
                b.Property<string>("UserId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("UserType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("HighwayId");
                b.ToTable("UserProfiles");
            });

            modelBuilder.Entity("AirwaysMergeSafeServer.Models.VehicleEvent", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<double?>("AltitudeMeters").HasColumnType("REAL");
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("DeviceId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("EventType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
                b.Property<string>("HighwayId").IsRequired().HasMaxLength(50).HasColumnType("TEXT");
                b.Property<double?>("Latitude").HasColumnType("REAL");
                b.Property<double?>("Longitude").HasColumnType("REAL");
                b.Property<string>("Payload").HasMaxLength(500).HasColumnType("TEXT");
                b.Property<double?>("SpeedMph").HasColumnType("REAL");
                b.Property<string>("VehicleCategory").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
                b.Property<string>("VehicleClassJson").HasMaxLength(800).HasColumnType("TEXT");
                b.Property<string>("VehicleId").HasMaxLength(50).HasColumnType("TEXT");
                b.Property<string>("VehicleMode").IsRequired().HasMaxLength(10).HasColumnType("TEXT");
                b.Property<string>("ZoneId").HasMaxLength(50).HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("CreatedDate");
                b.HasIndex("HighwayId");
                b.HasIndex("VehicleCategory");
                b.HasIndex("VehicleMode");
                b.HasIndex("ZoneId");
                b.ToTable("VehicleEvents");
            });
#pragma warning restore 612, 618
        }
    }
}
