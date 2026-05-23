using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class RoleRpeository : IRoleRepository
    {
        private readonly StudentDbcontext _dbcontext;

        public RoleRpeository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // get all roles
        public async Task<List<Roles>> GetAllRolesAsync()
        {
            return await _dbcontext.Roles
                .ToListAsync();
        }

        // get role by id
        public async Task<Roles?> GetRoleByIdAsync(int id)
        {
            return await _dbcontext.Roles
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        // get role by name
        public async Task<Roles?> GetRoleByNameAsync(
            string roleName)
        {
            return await _dbcontext.Roles
                .Where(r => r.RoleName.ToLower()
                    .Equals(roleName.ToLower()))
                .FirstOrDefaultAsync();
        }

        // create role
        public async Task<Roles> CreateRoleAsync(Roles role)
        {
            await _dbcontext.Roles.AddAsync(role);
            await _dbcontext.SaveChangesAsync();
            return role;
        }

        // delete role
        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await GetRoleByIdAsync(id);
            if (role == null) return false;

            _dbcontext.Roles.Remove(role);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        // check duplicate - case insensitive
        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _dbcontext.Roles
                .AnyAsync(r => r.RoleName.ToLower()
                    .Equals(roleName.ToLower()));
        }
    }
}
