using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirwaysMergeSafeServer.Migrations
{
    /// <summary>
    /// Adds:
    ///   1. DataInputType column to InputFormatConfigs (values: physical, sensors,
    ///      satellite, telcom, tagtracker, automobile, airflycar). Default "".
    ///   2. Seeds three Automobile / GPS format configs.
    /// </summary>
    public partial class AddAutomobileAndDataInputType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataInputType",
                table: "InputFormatConfigs",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            // Seed automobile format configs
            migrationBuilder.InsertData(
                table: "InputFormatConfigs",
                columns: new[] { "FormatName", "SourceId", "SourceType", "DataInputType", "InputSource", "Description", "EnabledFieldsRaw", "CreatedDate" },
                values: new object[,]
                {
                    {
                        "OBD-II Live Stream",
                        "AUTO-OBD2-001",
                        "automobile",
                        "automobile",
                        "obd2://can0@192.168.1.50:35000",
                        "Real-time OBD-II diagnostics via CAN bus bridge — engine telemetry, fuel, RPM",
                        "vehicle_id,timestamp,latitude,longitude,speed_mph,heading,direction,lane,vehicle_type,event_type,zone_id,highway_id,satellite_count,hdop,isAirFlyCar,vin,make,model,year,odometer_km,engine_temp_c,fuel_level_pct,rpm,gear,throttle_pct,brake_pct,battery_voltage,abs_active,traction_control,obd_code",
                        "2026-06-22T00:00:00Z"
                    },
                    {
                        "Connected Vehicle GPS Feed",
                        "AUTO-GPS-002",
                        "automobile",
                        "automobile",
                        "https://cv-platform.example.com/api/v2/vehicles/stream",
                        "GPS + satellite positioning feed from connected vehicle telematics unit",
                        "vehicle_id,timestamp,latitude,longitude,altitude_m,speed_mph,heading,direction,lane,vehicle_type,event_type,zone_id,highway_id,satellite_count,hdop,isAirFlyCar,make,model,year",
                        "2026-06-22T00:00:00Z"
                    },
                    {
                        "Tyre Pressure Monitor System",
                        "AUTO-TPMS-003",
                        "automobile",
                        "automobile",
                        "tpms://rf433@unit-03.local",
                        "TPMS sensor data with GPS location — all four tyres, battery voltage",
                        "vehicle_id,timestamp,latitude,longitude,speed_mph,heading,vehicle_type,event_type,zone_id,highway_id,isAirFlyCar,vin,make,model,tire_pressure_fl,tire_pressure_fr,tire_pressure_rl,tire_pressure_rr,battery_voltage",
                        "2026-06-22T00:00:00Z"
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("InputFormatConfigs", "SourceId", new object[] { "AUTO-OBD2-001", "AUTO-GPS-002", "AUTO-TPMS-003" });
            migrationBuilder.DropColumn("DataInputType", "InputFormatConfigs");
        }
    }
}
