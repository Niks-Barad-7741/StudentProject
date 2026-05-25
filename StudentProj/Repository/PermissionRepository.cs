using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly StudentDbcontext _dbcontext;

        public PermissionRepository(StudentDbcontext dbcontext) 
        {
            _dbcontext = dbcontext;
        }
        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            var exists = await _dbcontext.RolePermissions
                           .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);

            if (exists) return false;
            var rolePermission = new RolePermissions
            {
                RoleId = roleId,
                PermissionId = permissionId
            };
            await _dbcontext.RolePermissions.AddAsync(rolePermission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<Permissions> CreatePermissionAsync(Permissions permission)
        {
            await _dbcontext.Permissions.AddAsync(permission);
            await _dbcontext.SaveChangesAsync();
            return permission;
        }

        public async Task<List<Permissions>> GetAllPermissionsAsync()
        {
            return await _dbcontext.Permissions
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async  Task<Permissions?> GetPermissionByIdAsync(int id)
        {
            return await _dbcontext.Permissions
                           .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Permissions?> GetPermissionByNameAsync(string name)
        {
            return await _dbcontext.Permissions
                           .FirstOrDefaultAsync(p => p.PermissionName.ToLower() == name.ToLower() && !p.IsDeleted);
        }

        public async Task<List<string>> GetPermissionsByRoleIdAsync(List<int> roleIds)
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

        public async Task<List<string>> GetPermissionsByRoleNamesAsync(List<string> roleNames)
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
            permission.DeletedAt = DateTime.UtcNow;
            _dbcontext.Permissions.Update(permission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermission = await _dbcontext.RolePermissions
        .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);
            if (rolePermission == null) return false;
            rolePermission.IsDeleted = true;
            rolePermission.DeletedAt = DateTime.UtcNow;
            _dbcontext.RolePermissions.Update(rolePermission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
