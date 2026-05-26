using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class RenamePermissionsToPrivileges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename the main Permissions table to Privileges
            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Privileges");

            // 2. Rename the PermissionName column inside the Privileges table
            migrationBuilder.RenameColumn(
                table: "Privileges",
                name: "PermissionName",
                newName: "PrivilegeName");

            // 3. Rename the RolePermissions table to RolePrivileges
            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePrivileges");

            // 4. Rename the PermissionId column inside RolePrivileges to PrivilegeId
            migrationBuilder.RenameColumn(
                table: "RolePrivileges",
                name: "PermissionId",
                newName: "PrivilegeId");

            // 5. Rename the index names to match the new schema
            migrationBuilder.RenameIndex(
                table: "RolePrivileges",
                name: "IX_RolePermissions_PermissionId",
                newName: "IX_RolePrivileges_PrivilegeId");

            migrationBuilder.RenameIndex(
                table: "RolePrivileges",
                name: "IX_RolePermissions_RoleId",
                newName: "IX_RolePrivileges_RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert all changes in reverse order if you ever roll back this migration
            migrationBuilder.RenameIndex(
                table: "RolePermissions",
                name: "IX_RolePrivileges_RoleId",
                newName: "IX_RolePermissions_RoleId");

            migrationBuilder.RenameIndex(
                table: "RolePermissions",
                name: "IX_RolePrivileges_PrivilegeId",
                newName: "IX_RolePermissions_PermissionId");

            migrationBuilder.RenameColumn(
                table: "RolePermissions",
                name: "PrivilegeId",
                newName: "PermissionId");

            migrationBuilder.RenameTable(
                name: "RolePrivileges",
                newName: "RolePermissions");

            migrationBuilder.RenameColumn(
                table: "Permissions",
                name: "PrivilegeName",
                newName: "PermissionName");

            migrationBuilder.RenameTable(
                name: "Privileges",
                newName: "Permissions");
        }
    }
}