using System.Collections.Generic;
using System.Threading.Tasks;
using StudentProj.Models;

namespace StudentProj.Repository_Interface
{
    public interface IPermissionRepository
    {
        Task<bool> HasPermissionAsync(int userId, string action, string menuName);
        Task<List<Permissions>> GetAllPermissionAsync();
        Task<Permissions?> GetPermissionByIdAsync(int id);
        Task<Permissions?> GetPermissionByNameAsync(string name);
        Task<bool> PermissionExistsAsync(string name);
        Task<Permissions> CreatePermissionAsync(Permissions permission);

        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId, int menuId);
        Task<List<string>> GetPermissionByRoleIdAsync(List<int> roleIds);
        Task<List<string>> GetPermissionByRoleNamesAsync(List<string> roleNames);

        Task<bool> UpdatePermissionRoleAsync(int id, Permissions permission);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId, int menuId);
    }
}

