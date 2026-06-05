using StudentProj.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentProj.Repository_Interface
{
    public interface IRoutePermissionRepository
    {
        Task<List<RoutePermissions>> GetAllRoutePermissionsAsync();
        Task<RoutePermissions?> GetRoutePermissionByIdAsync(int id);
        Task<RoutePermissions> CreateRoutePermissionAsync(RoutePermissions routePermission);
        Task<bool> UpdateRoutePermissionAsync(int id, RoutePermissions routePermission);
        Task<bool> DeleteRoutePermissionAsync(int id);
        Task<bool> RoutePermissionExistsAsync(string httpMethod, string pathPattern);
    }
}
