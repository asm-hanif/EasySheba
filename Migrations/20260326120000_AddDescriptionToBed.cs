using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasySheba.Migrations
{
    public partial class AddDescriptionToBed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Beds",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Beds");
        }
    }
}
