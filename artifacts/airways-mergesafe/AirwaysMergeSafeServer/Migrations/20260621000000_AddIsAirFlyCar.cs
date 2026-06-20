using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirwaysMergeSafeServer.Migrations
{
    /// <summary>
    /// Task 10: Adds IsAirFlyCar (Y/N) column to VehicleEvents.
    /// Default "N" — backward-compatible with all existing rows.
    /// </summary>
    public partial class AddIsAirFlyCar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsAirFlyCar",
                table: "VehicleEvents",
                type: "TEXT",
                maxLength: 1,
                nullable: false,
                defaultValue: "N");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("IsAirFlyCar", "VehicleEvents");
        }
    }
}
