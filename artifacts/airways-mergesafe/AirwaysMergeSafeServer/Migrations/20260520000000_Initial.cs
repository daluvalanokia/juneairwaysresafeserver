using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirwaysMergeSafeServer.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Highways",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    State = table.Column<string>(maxLength: 50, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Highways", x => x.Id));

            migrationBuilder.CreateTable(
                name: "MergeZones",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ZoneName = table.Column<string>(maxLength: 100, nullable: false),
                    ZoneId = table.Column<string>(maxLength: 50, nullable: false),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    MileMarker = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    GeofenceRadius = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_MergeZones", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SwitchServers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServerName = table.Column<string>(maxLength: 100, nullable: false),
                    ServerId = table.Column<string>(maxLength: 50, nullable: false),
                    ZoneId = table.Column<string>(maxLength: 50, nullable: true),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(maxLength: 45, nullable: true),
                    Port = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: false),
                    FirmwareVersion = table.Column<string>(maxLength: 20, nullable: true),
                    UptimeSeconds = table.Column<long>(nullable: false),
                    CpuPercent = table.Column<double>(nullable: false),
                    MemoryPercent = table.Column<double>(nullable: false),
                    LastHeartbeat = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_SwitchServers", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SensorDevices",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceName = table.Column<string>(maxLength: 100, nullable: false),
                    DeviceId = table.Column<string>(maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(maxLength: 50, nullable: false),
                    ZoneId = table.Column<string>(maxLength: 50, nullable: true),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    MileMarker = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Status = table.Column<string>(maxLength: 30, nullable: false),
                    FirmwareVersion = table.Column<string>(maxLength: 20, nullable: true),
                    LastHeartbeat = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_SensorDevices", x => x.Id));

            migrationBuilder.CreateTable(
                name: "TriangulationConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ZoneId = table.Column<string>(maxLength: 50, nullable: false),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    GeofenceRadius = table.Column<int>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Switch1Label = table.Column<string>(maxLength: 50, nullable: true),
                    Switch1ServerId = table.Column<string>(maxLength: 50, nullable: true),
                    Switch1Lat = table.Column<double>(nullable: true),
                    Switch1Lon = table.Column<double>(nullable: true),
                    Switch2Label = table.Column<string>(maxLength: 50, nullable: true),
                    Switch2ServerId = table.Column<string>(maxLength: 50, nullable: true),
                    Switch2Lat = table.Column<double>(nullable: true),
                    Switch2Lon = table.Column<double>(nullable: true),
                    Switch3Label = table.Column<string>(maxLength: 50, nullable: true),
                    Switch3ServerId = table.Column<string>(maxLength: 50, nullable: true),
                    Switch3Lat = table.Column<double>(nullable: true),
                    Switch3Lon = table.Column<double>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_TriangulationConfigs", x => x.Id));

            migrationBuilder.CreateTable(
                name: "VehicleEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<string>(maxLength: 30, nullable: false),
                    ZoneId = table.Column<string>(maxLength: 50, nullable: true),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(maxLength: 50, nullable: true),
                    VehicleId = table.Column<string>(maxLength: 50, nullable: true),
                    SpeedMph = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Payload = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_VehicleEvents", x => x.Id));

            migrationBuilder.CreateTable(
                name: "InputFormatConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormatName = table.Column<string>(maxLength: 100, nullable: false),
                    SourceId = table.Column<string>(maxLength: 50, nullable: true),
                    SourceType = table.Column<string>(maxLength: 30, nullable: false),
                    InputSource = table.Column<string>(maxLength: 300, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    EnabledFieldsRaw = table.Column<string>(maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_InputFormatConfigs", x => x.Id));

            migrationBuilder.CreateTable(
                name: "SamplePayloads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfigId = table.Column<int>(nullable: true),
                    SourceType = table.Column<string>(maxLength: 30, nullable: false),
                    Label = table.Column<string>(maxLength: 100, nullable: true),
                    Payload = table.Column<string>(nullable: true),
                    IsValid = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_SamplePayloads", x => x.Id));

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(maxLength: 50, nullable: true),
                    FullName = table.Column<string>(maxLength: 100, nullable: false),
                    UserType = table.Column<string>(maxLength: 30, nullable: false),
                    Phone = table.Column<string>(maxLength: 20, nullable: true),
                    Address = table.Column<string>(maxLength: 200, nullable: true),
                    HighwayId = table.Column<string>(maxLength: 50, nullable: true),
                    HighwayName = table.Column<string>(maxLength: 100, nullable: true),
                    DeviceIdsRaw = table.Column<string>(maxLength: 500, nullable: true),
                    Notes = table.Column<string>(maxLength: 300, nullable: true),
                    Password = table.Column<string>(maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_UserProfiles", x => x.Id));

            migrationBuilder.CreateIndex(name: "IX_MergeZones_HighwayId",     table: "MergeZones",         column: "HighwayId");
            migrationBuilder.CreateIndex(name: "IX_SwitchServers_HighwayId",  table: "SwitchServers",       column: "HighwayId");
            migrationBuilder.CreateIndex(name: "IX_SwitchServers_ZoneId",     table: "SwitchServers",       column: "ZoneId");
            migrationBuilder.CreateIndex(name: "IX_SensorDevices_HighwayId",  table: "SensorDevices",       column: "HighwayId");
            migrationBuilder.CreateIndex(name: "IX_SensorDevices_ZoneId",     table: "SensorDevices",       column: "ZoneId");
            migrationBuilder.CreateIndex(name: "IX_VehicleEvents_HighwayId",  table: "VehicleEvents",       column: "HighwayId");
            migrationBuilder.CreateIndex(name: "IX_VehicleEvents_ZoneId",     table: "VehicleEvents",       column: "ZoneId");
            migrationBuilder.CreateIndex(name: "IX_VehicleEvents_CreatedDate",table: "VehicleEvents",       column: "CreatedDate");
            migrationBuilder.CreateIndex(name: "IX_InputFormatConfigs_SourceType", table: "InputFormatConfigs", column: "SourceType");
            migrationBuilder.CreateIndex(name: "IX_UserProfiles_HighwayId",   table: "UserProfiles",        column: "HighwayId");

            // Composite indexes (HighwayId + ZoneId)
            migrationBuilder.CreateIndex(
                name:    "IX_SwitchServers_HighwayId_ZoneId",
                table:   "SwitchServers",
                columns: new[] { "HighwayId", "ZoneId" });
            migrationBuilder.CreateIndex(
                name:    "IX_SensorDevices_HighwayId_ZoneId",
                table:   "SensorDevices",
                columns: new[] { "HighwayId", "ZoneId" });
            migrationBuilder.CreateIndex(
                name:    "IX_VehicleEvents_HighwayId_ZoneId",
                table:   "VehicleEvents",
                columns: new[] { "HighwayId", "ZoneId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop composite indexes first
            migrationBuilder.DropIndex(name: "IX_SwitchServers_HighwayId_ZoneId",  table: "SwitchServers");
            migrationBuilder.DropIndex(name: "IX_SensorDevices_HighwayId_ZoneId",  table: "SensorDevices");
            migrationBuilder.DropIndex(name: "IX_VehicleEvents_HighwayId_ZoneId",  table: "VehicleEvents");

            migrationBuilder.DropTable(name: "Highways");
            migrationBuilder.DropTable(name: "MergeZones");
            migrationBuilder.DropTable(name: "SwitchServers");
            migrationBuilder.DropTable(name: "SensorDevices");
            migrationBuilder.DropTable(name: "TriangulationConfigs");
            migrationBuilder.DropTable(name: "VehicleEvents");
            migrationBuilder.DropTable(name: "InputFormatConfigs");
            migrationBuilder.DropTable(name: "SamplePayloads");
            migrationBuilder.DropTable(name: "UserProfiles");
        }
    }
}
