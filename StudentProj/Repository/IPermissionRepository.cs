using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IPermissionRepository
    {
        Task<List<Permissions>> GetAllPermissionsAsync();
        Task<Permissions?> GetPermissionByIdAsync(int id);
        Task<Permissions?> GetPermissionByNameAsync(string name);
        Task<bool> PermissionExistsAsync(string name);
        Task<Permissions> CreatePermissionAsync(Permissions permission);

        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<List<string>> GetPermissionsByRoleIdAsync(List<int> roleIds);
        Task<List<string>> GetPermissionsByRoleNamesAsync(List<string> roleNames);

        Task<bool> UpdatePermissionRoleAsync(int id, Permissions permission);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);

    }
}
