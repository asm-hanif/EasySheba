using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySheba.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAppointmentLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyAppointmentLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FridayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MondayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaturdayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SundayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThursdayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TuesdayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WednesdayLimit",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCountedTowardsLimit",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyAppointmentLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "FridayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "MondayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SaturdayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SundayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ThursdayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "TuesdayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "WednesdayLimit",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IsCountedTowardsLimit",
                table: "Appointments");
        }
    }
}
