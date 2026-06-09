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
                // _cache.Remove($"Permissions_Role_{role.RoleName}");
                await _cache.RemoveAsync($"Permissions_Role_{role.RoleName}");
            }
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId, int menuId)
        {
            var existing = await _dbcontext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId 
                                        && rp.PermissionId == permissionId 
                                        && (rp.MenuId == menuId || (menuId == 0 && rp.MenuId == null)));

            if (existing != null)
            {
                if (!existing.IsDeleted) 
                {
                    return false;
                }

                existing.IsDeleted = false;
                existing.DeletedAt = null;
                _dbcontext.RolePermissions.Update(existing);
                await _dbcontext.SaveChangesAsync();
                await ClearRoleCacheAsync(roleId);
                return true;
            }

            var rolePermission = new RolePermissions
            {
                RoleId = roleId,
                PermissionId = permissionId,
                MenuId = menuId == 0 ? null : menuId
            };
            await _dbcontext.RolePermissions.AddAsync(rolePermission);
            await _dbcontext.SaveChangesAsync();
            await ClearRoleCacheAsync(roleId);
            return true;
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

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, int menuId)
        {
            var rolePermission = await _dbcontext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId 
                                        && rp.PermissionId == permissionId 
                                        && (rp.MenuId == menuId || (menuId == 0 && rp.MenuId == null)) 
                                        && !rp.IsDeleted);
            if (rolePermission == null) return false;
            rolePermission.IsDeleted = true;
            rolePermission.DeletedAt = DateTime.Now;
            _dbcontext.RolePermissions.Update(rolePermission);
            await _dbcontext.SaveChangesAsync();
            await ClearRoleCacheAsync(roleId);
            return true;
        }
    }
}
