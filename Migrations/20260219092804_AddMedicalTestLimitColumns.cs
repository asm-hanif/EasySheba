using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySheba.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalTestLimitColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyAppointmentLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FridayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MondayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaturdayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SundayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThursdayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TuesdayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WednesdayLimit",
                table: "MedicalTests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTestCountedTowardsLimit",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MedicalTestId1",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MedicalTestId1",
                table: "Appointments",
                column: "MedicalTestId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_MedicalTests_MedicalTestId1",
                table: "Appointments",
                column: "MedicalTestId1",
                principalTable: "MedicalTests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_MedicalTests_MedicalTestId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_MedicalTestId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DailyAppointmentLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "FridayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "MondayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "SaturdayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "SundayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "ThursdayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "TuesdayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "WednesdayLimit",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "IsTestCountedTowardsLimit",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MedicalTestId1",
                table: "Appointments");
        }
    }
}
