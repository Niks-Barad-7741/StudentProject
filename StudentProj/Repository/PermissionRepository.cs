using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using Microsoft.Extensions.Caching.Distributed;

namespace StudentProj.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IDistributedCache _cache;

        public PermissionRepository(StudentDbcontext dbcontext, IDistributedCache cache)
        {
            _dbcontext = dbcontext;
            _cache = cache;
        }

        public async Task<bool> HasPermissionAsync(int userId, string action, string menuName)
        {
            return await _dbcontext.RolePermissions
                .Where(rp => !rp.IsDeleted
                    && !rp.Role.IsDeleted
                    && !rp.Permission.IsDeleted
                    && rp.Menu != null && !rp.Menu.IsDeleted
                    )
                .Where(rp => rp.Permission.PermissionName.ToLower() == action.ToLower()
                          && rp.Menu.MenuName.ToLower() == menuName.ToLower())
                .Where(rp => _dbcontext.StudentRoles
                    .Any(sr => sr.StudentId == userId && sr.RoleId == rp.RoleId && !sr.IsDeleted))
                .AnyAsync();
        }

        private async Task ClearRoleCacheAsync(int roleId)
        {
            var role = await _dbcontext.Roles.FindAsync(roleId);
            if (role != null)
            {
                await _cache.RemoveAsync($"Permissions_Role_{role.RoleName}");
            }
        }

        // New Helper: Get Permission IDs to cascade down (for assign) or up (for revoke)
        private List<int> GetCascadePermissionIds(int permissionId, bool isAssign)
        {
            // Hierarchy: read (10) <- create (9) <- update (11) <- delete (12)
            if (isAssign)
            {
                return permissionId switch
                {
                    12 => new List<int> { 11, 9, 10 }, // delete assigns update, create, read
                    11 => new List<int> { 9, 10 },     // update assigns create, read
                    9 => new List<int> { 10 },       // create assigns read
                    _ => new List<int>()
                };
            }
            else
            {
                return permissionId switch
                {
                    10 => new List<int> { 9, 11, 12 }, // revoking read revokes create, update, delete
                    9 => new List<int> { 11, 12 },     // revoking create revokes update, delete
                    11 => new List<int> { 12 },        // revoking update revokes delete
                    _ => new List<int>()
                };
            }
        }

        // New Helper: Find fine-grained menu
        private async Task<int?> FindFineGrainedMenuIdAsync(string parentMenuName, int permissionId)
        {
            // Map permission ID to action prefix
            string action = permissionId switch
            {
                9 => "create",
                10 => "read",
                11 => "update",
                12 => "delete",
                _ => null
            };

            if (action == null) return null;

            // Try {action}-{parentMenuName} e.g. create-course
            string exactName = $"{action}-{parentMenuName}".ToLower();
            var menu = await _dbcontext.Menus.FirstOrDefaultAsync(m => m.MenuName.ToLower() == exactName && !m.IsDeleted);
            if (menu != null) return menu.Id;

            // Try {action}-{singular} e.g. create-student (from students)
            if (parentMenuName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                string singularName = $"{action}-{parentMenuName.Substring(0, parentMenuName.Length - 1)}".ToLower();
                var menuSingular = await _dbcontext.Menus.FirstOrDefaultAsync(m => m.MenuName.ToLower() == singularName && !m.IsDeleted);
                if (menuSingular != null) return menuSingular.Id;
            }

            return null;
        }

        // Extracted single assign logic
        private async Task<bool> AssignSinglePermissionAsync(int roleId, int permissionId, int? menuId)
        {
            var existing = await _dbcontext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId
                                        && rp.PermissionId == permissionId
                                        && rp.MenuId == menuId);

            if (existing != null)
            {
                if (!existing.IsDeleted) return false;

                existing.IsDeleted = false;
                existing.DeletedAt = null;
                _dbcontext.RolePermissions.Update(existing);
                return true;
            }

            var rolePermission = new RolePermissions
            {
                RoleId = roleId,
                PermissionId = permissionId,
                MenuId = menuId
            };
            await _dbcontext.RolePermissions.AddAsync(rolePermission);
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId, int menuId)
        {
            bool anyAssigned = false;

            // 1. Assign the original permission (with its original menuId)
            int? nullableMenuId = menuId == 0 ? null : menuId;
            if (await AssignSinglePermissionAsync(roleId, permissionId, nullableMenuId))
                anyAssigned = true;

            // 2. Cascade Logic
            if (nullableMenuId.HasValue)
            {
                var menu = await _dbcontext.Menus.FindAsync(nullableMenuId.Value);
                if (menu != null && !menu.IsDeleted)
                {
                    string menuName = menu.MenuName.ToLower();
                    bool isParentMenu = !menuName.StartsWith("create-") &&
                                        !menuName.StartsWith("read-") &&
                                        !menuName.StartsWith("update-") &&
                                        !menuName.StartsWith("delete-");

                    if (isParentMenu)
                    {
                        // Assign original permission on fine-grained menu
                        int? fineGrainedMenuId = await FindFineGrainedMenuIdAsync(menuName, permissionId);
                        if (fineGrainedMenuId.HasValue)
                        {
                            if (await AssignSinglePermissionAsync(roleId, permissionId, fineGrainedMenuId.Value))
                                anyAssigned = true;
                        }

                        // Get permissions to cascade down
                        var cascadePerms = GetCascadePermissionIds(permissionId, isAssign: true);
                        foreach (var cascadePermId in cascadePerms)
                        {
                            // Assign on parent menu
                            if (await AssignSinglePermissionAsync(roleId, cascadePermId, nullableMenuId.Value))
                                anyAssigned = true;

                            // Assign on fine-grained menu
                            int? cascadeFineGrainedId = await FindFineGrainedMenuIdAsync(menuName, cascadePermId);
                            if (cascadeFineGrainedId.HasValue)
                            {
                                if (await AssignSinglePermissionAsync(roleId, cascadePermId, cascadeFineGrainedId.Value))
                                    anyAssigned = true;
                            }
                        }
                    }
                }
            }

            if (anyAssigned)
            {
                await _dbcontext.SaveChangesAsync();
                await ClearRoleCacheAsync(roleId);
            }

            return anyAssigned;
        }

        public async Task<Permissions> CreatePermissionAsync(Permissions permission)
        {
            var existing = await _dbcontext.Permissions
                .FirstOrDefaultAsync(p => p.PermissionName.ToLower() == permission.PermissionName.ToLower());

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    _dbcontext.Permissions.Update(existing);
                    await _dbcontext.SaveChangesAsync();
                }
                return existing;
            }

            await _dbcontext.Permissions.AddAsync(permission);
            await _dbcontext.SaveChangesAsync();
            return permission;
        }

        public async Task<List<Permissions>> GetAllPermissionAsync()
        {
            return await _dbcontext.Permissions
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Permissions?> GetPermissionByIdAsync(int id)
        {
            return await _dbcontext.Permissions
                           .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Permissions?> GetPermissionByNameAsync(string name)
        {
            return await _dbcontext.Permissions
                           .FirstOrDefaultAsync(p => p.PermissionName.ToLower() == name.ToLower() && !p.IsDeleted);
        }

        public async Task<List<string>> GetPermissionByRoleIdAsync(List<int> roleIds)
        {
            return await _dbcontext.RolePermissions
                           .Where(rp => roleIds.Contains(rp.RoleId)
                                        && !rp.IsDeleted
                                        && !rp.Role.IsDeleted
                                        && !rp.Permission.IsDeleted)
                           .Select(rp => rp.Permission.PermissionName)
                           .Distinct()
                           .ToListAsync();
        }

        public async Task<bool> PermissionExistsAsync(string name)
        {
            return await _dbcontext.Permissions
                             .AnyAsync(p => p.PermissionName.ToLower() == name.ToLower() && !p.IsDeleted);
        }

        public async Task<List<string>> GetPermissionByRoleNamesAsync(List<string> roleNames)
        {
            return await _dbcontext.RolePermissions
                .Where(rp => roleNames.Contains(rp.Role.RoleName)
                             && !rp.IsDeleted
                             && !rp.Role.IsDeleted
                             && !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> UpdatePermissionRoleAsync(int id, Permissions permission)
        {
            _dbcontext.Permissions.Update(permission);
            await _dbcontext.SaveChangesAsync();
            return permission.Id == id;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var permission = await GetPermissionByIdAsync(id);
            if (permission == null) return false;
            permission.IsDeleted = true;
            permission.DeletedAt = DateTime.Now;
            _dbcontext.Permissions.Update(permission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        // Extracted single remove logic
        private async Task<bool> RemoveSinglePermissionAsync(int roleId, int permissionId, int? menuId)
        {
            var rolePermission = await _dbcontext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId
                                        && rp.PermissionId == permissionId
                                        && rp.MenuId == menuId
                                        && !rp.IsDeleted);
            if (rolePermission == null) return false;

            rolePermission.IsDeleted = true;
            rolePermission.DeletedAt = DateTime.Now;
            _dbcontext.RolePermissions.Update(rolePermission);
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, int menuId)
        {
            bool anyRevoked = false;
            int? nullableMenuId = menuId == 0 ? null : menuId;

            // 1. Revoke the original permission
            if (await RemoveSinglePermissionAsync(roleId, permissionId, nullableMenuId))
                anyRevoked = true;

            // 2. Cascade Logic
            if (nullableMenuId.HasValue)
            {
                var menu = await _dbcontext.Menus.FindAsync(nullableMenuId.Value);
                if (menu != null && !menu.IsDeleted)
                {
                    string menuName = menu.MenuName.ToLower();
                    bool isParentMenu = !menuName.StartsWith("create-") &&
                                        !menuName.StartsWith("read-") &&
                                        !menuName.StartsWith("update-") &&
                                        !menuName.StartsWith("delete-");

                    if (isParentMenu)
                    {
                        // Revoke original permission from fine-grained menu
                        int? fineGrainedMenuId = await FindFineGrainedMenuIdAsync(menuName, permissionId);
                        if (fineGrainedMenuId.HasValue)
                        {
                            if (await RemoveSinglePermissionAsync(roleId, permissionId, fineGrainedMenuId.Value))
                                anyRevoked = true;
                        }

                        // Get permissions to cascade up
                        var cascadePerms = GetCascadePermissionIds(permissionId, isAssign: false);
                        foreach (var cascadePermId in cascadePerms)
                        {
                            // Revoke from parent menu
                            if (await RemoveSinglePermissionAsync(roleId, cascadePermId, nullableMenuId.Value))
                                anyRevoked = true;

                            // Revoke from fine-grained menu
                            int? cascadeFineGrainedId = await FindFineGrainedMenuIdAsync(menuName, cascadePermId);
                            if (cascadeFineGrainedId.HasValue)
                            {
                                if (await RemoveSinglePermissionAsync(roleId, cascadePermId, cascadeFineGrainedId.Value))
                                    anyRevoked = true;
                            }
                        }
                    }
                }
            }

            if (anyRevoked)
            {
                await _dbcontext.SaveChangesAsync();
                await ClearRoleCacheAsync(roleId);
            }

            return anyRevoked;
        }
    }
}
