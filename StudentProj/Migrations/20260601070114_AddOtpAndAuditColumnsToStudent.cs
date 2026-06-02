using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpAndAuditColumnsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetOtp",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetOtpExpiry",
                table: "Student",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Student",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetOtp",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "ResetOtpExpiry",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Student");
        }
    }
}
