using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class PrivilegeRepository : IPrivilegeRepository
    {
        private readonly StudentDbcontext _dbcontext;

        public PrivilegeRepository(StudentDbcontext dbcontext) 
        {
            _dbcontext = dbcontext;
        }

        public async Task<bool> HasPermissionAsync(int userId, string action, string menuName)
        {
            return await _dbcontext.RolePrivileges
                .Where(rp => !rp.IsDeleted
                    && !rp.Role.IsDeleted
                    && !rp.Privilege.IsDeleted
                    && rp.Menu != null && !rp.Menu.IsDeleted)
                .Where(rp => rp.Privilege.PrivilegeName.ToLower() == action.ToLower() 
                          && rp.Menu.MenuName.ToLower() == menuName.ToLower())
                .Where(rp => _dbcontext.StudentRoles
                    .Any(sr => sr.StudentId == userId && sr.RoleId == rp.RoleId))
                .AnyAsync();
        }

        public async Task<bool> AssignPrivilegeToRoleAsync(int roleId, int permissionId, int menuId)
        {
            var exists = await _dbcontext.RolePrivileges
                           .AnyAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == permissionId && rp.MenuId == menuId && !rp.IsDeleted);

            if (exists) return false;
            var rolePermission = new RolePrivileges
            {
                RoleId = roleId,
                PrivilegeId = permissionId,
                MenuId = menuId
            };
            await _dbcontext.RolePrivileges.AddAsync(rolePermission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<Privileges> CreatePrivilegeAsync(Privileges permission)
        {
            await _dbcontext.Privileges.AddAsync(permission);
            await _dbcontext.SaveChangesAsync();
            return permission;
        }

        public async Task<List<Privileges>> GetAllPrivilegeAsync()
        {
            return await _dbcontext.Privileges
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async  Task<Privileges?> GetPrivilegeByIdAsync(int id)
        {
            return await _dbcontext.Privileges
                           .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Privileges?> GetPrivilegeByNameAsync(string name)
        {
            return await _dbcontext.Privileges
                           .FirstOrDefaultAsync(p => p.PrivilegeName.ToLower() == name.ToLower() && !p.IsDeleted);
        }

        public async Task<List<string>> GetPrivilegeByRoleIdAsync(List<int> roleIds)
        {
            return await _dbcontext.RolePrivileges
                           .Where(rp => roleIds.Contains(rp.RoleId)
                                        && !rp.IsDeleted
                                        && !rp.Role.IsDeleted
                                        && !rp.Privilege.IsDeleted)
                           .Select(rp => rp.Privilege.PrivilegeName)
                           .Distinct()
                           .ToListAsync();
        }

        public async Task<bool> PrivilegeExistsAsync(string name)
        {
            return await _dbcontext.Privileges
                            .AnyAsync(p => p.PrivilegeName.ToLower() == name.ToLower() && !p.IsDeleted);
        }

        public async Task<List<string>> GetPrivilegeByRoleNamesAsync(List<string> roleNames)
        {
            return await _dbcontext.RolePrivileges
                .Where(rp => roleNames.Contains(rp.Role.RoleName)
                             && !rp.IsDeleted
                             && !rp.Role.IsDeleted
                             && !rp.Privilege.IsDeleted)
                .Select(rp => rp.Privilege.PrivilegeName)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> UpdatePrivilegeRoleAsync(int id, Privileges permission) 
        {
            _dbcontext.Privileges.Update(permission);
            await _dbcontext.SaveChangesAsync();
            return permission.Id == id;
        }

        public async Task<bool> DeletePrivilegeAsync(int id) 
        {
            var permission = await GetPrivilegeByIdAsync(id);
            if (permission == null) return false;
            permission.IsDeleted = true;
            permission.DeletedAt = DateTime.Now;
            _dbcontext.Privileges.Update(permission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePrivilegeFromRoleAsync(int roleId, int permissionId, int menuId)
        {
            var rolePermission = await _dbcontext.RolePrivileges
        .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PrivilegeId == permissionId && rp.MenuId == menuId && !rp.IsDeleted);
            if (rolePermission == null) return false;
            rolePermission.IsDeleted = true;
            rolePermission.DeletedAt = DateTime.Now;
            _dbcontext.RolePrivileges.Update(rolePermission);
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
