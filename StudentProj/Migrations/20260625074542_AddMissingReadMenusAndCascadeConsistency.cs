using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProj.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingReadMenusAndCascadeConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- 1. Insert missing read-* menus
                INSERT INTO Menus (MenuName, MenuRoute, IsDeleted) VALUES 
                ('read-student', '/read-student', 0),
                ('read-role', '/read-role', 0),
                ('read-permission', '/read-permission', 0),
                ('read-menu', '/read-menu', 0),
                ('read-log', '/read-log', 0);

                -- 2. Update RoutePermissions
                UPDATE RoutePermissions SET RequiredMenuName = 'read-student' WHERE RequiredMenuName = 'students' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'read-role' WHERE RequiredMenuName = 'roles' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'read-permission' WHERE RequiredMenuName = 'permissions' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'read-menu' WHERE RequiredMenuName = 'menus' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'read-log' WHERE RequiredMenuName = 'logs' AND HttpMethod = 'GET';

                -- 3. Migrate existing RolePermissions
                -- Find the permission id for 'read'
                DECLARE @readPermId INT = (SELECT TOP 1 Id FROM Permissions WHERE PermissionName = 'read');

                IF @readPermId IS NOT NULL
                BEGIN
                    -- Copy read permission from parent menu 'students' to 'read-student'
                    INSERT INTO RolePermissions (RoleId, PermissionId, MenuId, IsDeleted)
                    SELECT rp.RoleId, rp.PermissionId, mNew.Id, rp.IsDeleted
                    FROM RolePermissions rp
                    JOIN Menus mOld ON rp.MenuId = mOld.Id
                    CROSS JOIN Menus mNew
                    WHERE mOld.MenuName = 'students' AND mNew.MenuName = 'read-student' AND rp.PermissionId = @readPermId
                    AND NOT EXISTS (SELECT 1 FROM RolePermissions rp2 WHERE rp2.RoleId = rp.RoleId AND rp2.PermissionId = rp.PermissionId AND rp2.MenuId = mNew.Id);

                    -- Same for roles
                    INSERT INTO RolePermissions (RoleId, PermissionId, MenuId, IsDeleted)
                    SELECT rp.RoleId, rp.PermissionId, mNew.Id, rp.IsDeleted
                    FROM RolePermissions rp
                    JOIN Menus mOld ON rp.MenuId = mOld.Id
                    CROSS JOIN Menus mNew
                    WHERE mOld.MenuName = 'roles' AND mNew.MenuName = 'read-role' AND rp.PermissionId = @readPermId
                    AND NOT EXISTS (SELECT 1 FROM RolePermissions rp2 WHERE rp2.RoleId = rp.RoleId AND rp2.PermissionId = rp.PermissionId AND rp2.MenuId = mNew.Id);

                    -- Same for permissions
                    INSERT INTO RolePermissions (RoleId, PermissionId, MenuId, IsDeleted)
                    SELECT rp.RoleId, rp.PermissionId, mNew.Id, rp.IsDeleted
                    FROM RolePermissions rp
                    JOIN Menus mOld ON rp.MenuId = mOld.Id
                    CROSS JOIN Menus mNew
                    WHERE mOld.MenuName = 'permissions' AND mNew.MenuName = 'read-permission' AND rp.PermissionId = @readPermId
                    AND NOT EXISTS (SELECT 1 FROM RolePermissions rp2 WHERE rp2.RoleId = rp.RoleId AND rp2.PermissionId = rp.PermissionId AND rp2.MenuId = mNew.Id);

                    -- Same for menus
                    INSERT INTO RolePermissions (RoleId, PermissionId, MenuId, IsDeleted)
                    SELECT rp.RoleId, rp.PermissionId, mNew.Id, rp.IsDeleted
                    FROM RolePermissions rp
                    JOIN Menus mOld ON rp.MenuId = mOld.Id
                    CROSS JOIN Menus mNew
                    WHERE mOld.MenuName = 'menus' AND mNew.MenuName = 'read-menu' AND rp.PermissionId = @readPermId
                    AND NOT EXISTS (SELECT 1 FROM RolePermissions rp2 WHERE rp2.RoleId = rp.RoleId AND rp2.PermissionId = rp.PermissionId AND rp2.MenuId = mNew.Id);

                    -- Same for logs
                    INSERT INTO RolePermissions (RoleId, PermissionId, MenuId, IsDeleted)
                    SELECT rp.RoleId, rp.PermissionId, mNew.Id, rp.IsDeleted
                    FROM RolePermissions rp
                    JOIN Menus mOld ON rp.MenuId = mOld.Id
                    CROSS JOIN Menus mNew
                    WHERE mOld.MenuName = 'logs' AND mNew.MenuName = 'read-log' AND rp.PermissionId = @readPermId
                    AND NOT EXISTS (SELECT 1 FROM RolePermissions rp2 WHERE rp2.RoleId = rp.RoleId AND rp2.PermissionId = rp.PermissionId AND rp2.MenuId = mNew.Id);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Restore RoutePermissions
                UPDATE RoutePermissions SET RequiredMenuName = 'students' WHERE RequiredMenuName = 'read-student' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'roles' WHERE RequiredMenuName = 'read-role' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'permissions' WHERE RequiredMenuName = 'read-permission' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'menus' WHERE RequiredMenuName = 'read-menu' AND HttpMethod = 'GET';
                UPDATE RoutePermissions SET RequiredMenuName = 'logs' WHERE RequiredMenuName = 'read-log' AND HttpMethod = 'GET';

                -- Delete RolePermissions for the new menus
                DELETE FROM RolePermissions WHERE MenuId IN (SELECT Id FROM Menus WHERE MenuName IN ('read-student', 'read-role', 'read-permission', 'read-menu', 'read-log'));

                -- Delete the menus
                DELETE FROM Menus WHERE MenuName IN ('read-student', 'read-role', 'read-permission', 'read-menu', 'read-log');
            ");
        }
    }
}
