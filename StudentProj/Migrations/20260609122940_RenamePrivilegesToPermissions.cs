using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class RenamePrivilegesToPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop old foreign keys (using exact DB constraint names)
            migrationBuilder.DropForeignKey(
                name: "FK_RolePrivileges_Menus_MenuId",
                table: "RolePrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePrivileges");

            // 2. Rename Tables
            migrationBuilder.RenameTable(
                name: "Privileges",
                newName: "Permissions");

            migrationBuilder.RenameTable(
                name: "RolePrivileges",
                newName: "RolePermissions");

            // 3. Rename Columns
            migrationBuilder.RenameColumn(
                name: "PrivilegeName",
                table: "Permissions",
                newName: "PermissionName");

            migrationBuilder.RenameColumn(
                name: "PrivilegeId",
                table: "RolePermissions",
                newName: "PermissionId");

            // Rename RoutePermissions column
            migrationBuilder.RenameColumn(
                name: "RequiredPrivilegeName",
                table: "RoutePermissions",
                newName: "RequiredPermissionName");

            // 4. Rename Indexes
            migrationBuilder.RenameIndex(
                name: "IX_RolePrivileges_MenuId",
                newName: "IX_RolePermissions_MenuId",
                table: "RolePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_RolePrivileges_PrivilegeId",
                newName: "IX_RolePermissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.RenameIndex(
                name: "IX_RolePrivileges_RoleId",
                newName: "IX_RolePermissions_RoleId",
                table: "RolePermissions");

            // 5. Add new foreign keys (with consistent naming convention)
            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Menus_MenuId",
                table: "RolePermissions",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 6. Update RoutePermissions path patterns from api/privileges to api/permissions
            migrationBuilder.Sql("UPDATE RoutePermissions SET PathPattern = REPLACE(PathPattern, 'api/privileges', 'api/permissions')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop new foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Menus_MenuId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePermissions");

            // 2. Rename Columns back
            migrationBuilder.RenameColumn(
                name: "PermissionId",
                table: "RolePermissions",
                newName: "PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "PermissionName",
                table: "Permissions",
                newName: "PrivilegeName");

            migrationBuilder.RenameColumn(
                name: "RequiredPermissionName",
                table: "RoutePermissions",
                newName: "RequiredPrivilegeName");

            // 3. Rename Tables back
            migrationBuilder.RenameTable(
                name: "Permissions",
                newName: "Privileges");

            migrationBuilder.RenameTable(
                name: "RolePermissions",
                newName: "RolePrivileges");

            // 4. Rename Indexes back
            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_MenuId",
                newName: "IX_RolePrivileges_MenuId",
                table: "RolePrivileges");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_PermissionId",
                newName: "IX_RolePrivileges_PrivilegeId",
                table: "RolePrivileges");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_RoleId",
                newName: "IX_RolePrivileges_RoleId",
                table: "RolePrivileges");

            // 5. Add old foreign keys back (matching original names)
            migrationBuilder.AddForeignKey(
                name: "FK_RolePrivileges_Menus_MenuId",
                table: "RolePrivileges",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionId",
                table: "RolePrivileges",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleId",
                table: "RolePrivileges",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // 6. Revert RoutePermissions path patterns from api/permissions to api/privileges
            migrationBuilder.Sql("UPDATE RoutePermissions SET PathPattern = REPLACE(PathPattern, 'api/permissions', 'api/privileges')");
        }
    }
}
