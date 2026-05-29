using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumnsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Student",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Student");
        }
    }
}
