using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirwaysMergeSafeServer.Migrations
{
    public partial class AddSwitchServerGpsLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GpsLocation",
                table: "SwitchServers",
                type: "TEXT",
                maxLength: 60,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GpsLocation",
                table: "SwitchServers");
        }
    }
}
