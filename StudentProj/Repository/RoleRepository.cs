using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using Microsoft.Extensions.Caching.Memory;

namespace StudentProj.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly StudentDbcontext _dbcontext;
        private readonly IMemoryCache _cache;

        public RoleRepository(StudentDbcontext dbcontext, IMemoryCache cache)
        {
            _dbcontext = dbcontext;
            _cache = cache;
        }

        // get all roles
        public async Task<List<Roles>> GetAllRolesAsync()
        {
            return await _dbcontext.Roles
                .Where(r => !r.IsDeleted)
                .ToListAsync();
        }

        // get role by id
        public async Task<Roles?> GetRoleByIdAsync(int id)
        {
            return await _dbcontext.Roles
                .Where(r => r.Id == id && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // get role by name
        public async Task<Roles?> GetRoleByNameAsync(
            string roleName)
        {
            return await _dbcontext.Roles
                .Where(r => r.RoleName.ToLower() 
                    .Equals(roleName.ToLower()) && !r.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // create role
        public async Task<Roles> CreateRoleAsync(Roles role)
        {
            var existing = await _dbcontext.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToLower() == role.RoleName.ToLower());

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    _dbcontext.Roles.Update(existing);
                    await _dbcontext.SaveChangesAsync();
                }
                return existing;
            }

            await _dbcontext.Roles.AddAsync(role);
            await _dbcontext.SaveChangesAsync();
            return role;
        }

        // delete role
        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role == null) return false;

            role.IsDeleted = true;
            role.DeletedAt = DateTime.Now;
            _dbcontext.Roles.Update(role);
            await _dbcontext.SaveChangesAsync();

            _cache.Remove($"Permissions_Role_{role.RoleName}");
            return true;
        }

        // check duplicate - case insensitive
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbcontext.Roles
                .AnyAsync(r => r.RoleName.ToLower()
                    .Equals(roleName.ToLower()) && !r.IsDeleted);
        }


        public async Task<bool> UpdateRoleAsync(int id,Roles role) 
        {
            var oldRole = await _dbcontext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (oldRole != null)
            {
                _cache.Remove($"Permissions_Role_{oldRole.RoleName}");
            }

            _dbcontext.Roles.Update(role);
            await _dbcontext.SaveChangesAsync();

            _cache.Remove($"Permissions_Role_{role.RoleName}");
            return role.Id == id;
        }
    }
}
