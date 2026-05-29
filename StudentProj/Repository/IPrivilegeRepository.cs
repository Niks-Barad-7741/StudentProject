using StudentProj.Models;

namespace StudentProj.Repository
{
    public interface IPrivilegeRepository
    {
        Task<bool> HasPermissionAsync(int userId, string action, string menuName);
        Task<List<Privileges>> GetAllPrivilegeAsync();
        Task<Privileges?> GetPrivilegeByIdAsync(int id);
        Task<Privileges?> GetPrivilegeByNameAsync(string name);
        Task<bool> PrivilegeExistsAsync(string name);
        Task<Privileges> CreatePrivilegeAsync(Privileges permission);

        Task<bool> AssignPrivilegeToRoleAsync(int roleId, int permissionId, int menuId);
        Task<List<string>> GetPrivilegeByRoleIdAsync(List<int> roleIds);
        Task<List<string>> GetPrivilegeByRoleNamesAsync(List<string> roleNames);

        Task<bool> UpdatePrivilegeRoleAsync(int id, Privileges permission);
        Task<bool> DeletePrivilegeAsync(int id);
        Task<bool> RemovePrivilegeFromRoleAsync(int roleId, int permissionId, int menuId);

    }
}

