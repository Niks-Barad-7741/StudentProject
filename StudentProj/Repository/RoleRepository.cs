using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly StudentDbcontext _dbcontext;

        public RoleRepository(StudentDbcontext dbcontext)
        {
            _dbcontext = dbcontext;
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
            return true;
            //_dbcontext.Roles.Remove(role);
            //await _dbcontext.SaveChangesAsync();
            //return true;
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
            _dbcontext.Roles.Update(role);
            await _dbcontext.SaveChangesAsync();
            return role.Id == id;
        }
    }
}
