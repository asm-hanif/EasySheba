using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySheba.Migrations
{
    /// <inheritdoc />
    public partial class AddWifiAvailableToBed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Doctors");

            migrationBuilder.AddColumn<bool>(
                name: "WifiAvailable",
                table: "Beds",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WifiAvailable",
                table: "Beds");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Doctors",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Doctors",
                type: "float",
                nullable: true);
        }
    }
}
