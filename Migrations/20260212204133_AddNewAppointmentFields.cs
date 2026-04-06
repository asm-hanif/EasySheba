using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySheba.Migrations
{
    /// <inheritdoc />
    public partial class AddNewAppointmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Qualification",
                table: "Doctors",
                newName: "Specialist");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Blogs",
                newName: "MediaPaths");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Blogs",
                newName: "Headline");

            migrationBuilder.AddColumn<string>(
                name: "AvailableDays",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvailableTime",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalLocation",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "MedicalTests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Degrees",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HospitalLocation",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Blogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "AcNonAc",
                table: "Beds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AttachedBathroom",
                table: "Beds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HospitalLocation",
                table: "Beds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "Beds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Beds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "Beds",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalBeds",
                table: "Beds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "TvAvailable",
                table: "Beds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Appointments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AppointmentType",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BedId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MedicalTestId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BedId",
                table: "Appointments",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MedicalTestId",
                table: "Appointments",
                column: "MedicalTestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Beds_BedId",
                table: "Appointments",
                column: "BedId",
                principalTable: "Beds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_MedicalTests_MedicalTestId",
                table: "Appointments",
                column: "MedicalTestId",
                principalTable: "MedicalTests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Beds_BedId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_MedicalTests_MedicalTestId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BedId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_MedicalTestId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AvailableDays",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "AvailableTime",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "HospitalLocation",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "MedicalTests");

            migrationBuilder.DropColumn(
                name: "Degrees",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "HospitalLocation",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "AcNonAc",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "AttachedBathroom",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "HospitalLocation",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "TotalBeds",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "TvAvailable",
                table: "Beds");

            migrationBuilder.DropColumn(
                name: "AppointmentType",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BedId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BookedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MedicalTestId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Specialist",
                table: "Doctors",
                newName: "Qualification");

            migrationBuilder.RenameColumn(
                name: "MediaPaths",
                table: "Blogs",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Headline",
                table: "Blogs",
                newName: "Content");

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
