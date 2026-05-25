using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IRoleRepository
    {
        // get all roles
        Task<List<Roles>> GetAllRolesAsync();

        // get role by id
        Task<Roles?> GetRoleByIdAsync(int id);

        // get role by name
        Task<Roles?> GetRoleByNameAsync(string roleName);

        // create role
        Task<Roles> CreateRoleAsync(Roles role);

        // delete role
        Task<bool> DeleteRoleAsync(int id);

        // check duplicate
        Task<bool> RoleExistsAsync(string roleName);

        Task<bool> UpdateRoleAsync(int id, Roles role);
    }
}
