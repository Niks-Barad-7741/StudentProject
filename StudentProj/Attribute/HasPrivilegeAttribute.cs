using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using StudentProj.Data;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Repository;

namespace StudentProj.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;
        private readonly string _menuName;

        public HasPermissionAttribute(string permission, string menuName)
        {
            _permission = permission;
            _menuName = menuName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                var failResponse = ApiResponse<object>.Create(ResponseStatus.Unauthorized);
                context.Result = new ObjectResult(failResponse) 
                { 
                    StatusCode = 401 
                };
                return;
            }

            // 2. Super Admin bypasses all permission checks
            if (user.IsInRole("Super Admin"))
            {
                return;
            }

            // 3. Extract UserId from JWT claims
            var userIdClaim = user.FindFirst("Id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                var failResponse = ApiResponse<object>.Create(ResponseStatus.Unauthorized);
                context.Result = new ObjectResult(failResponse) 
                { 
                    StatusCode = 401 
                };
                return;
            }

            // 4. Check permissions using in-memory Cache-Aside
            var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<StudentDbcontext>();

            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            string requiredPermission = $"{_permission}:{_menuName}";
            bool hasAccess = false;

            foreach (var role in roles)
            {
                string cacheKey = $"Permissions_Role_{role}";

                if (!cache.TryGetValue(cacheKey, out List<string>? rolePermissions) || rolePermissions == null)
                {
                    // Cache Miss: Query SQL Server Database for this role's active permissions
                    rolePermissions = await dbContext.RolePrivileges
                        .Where(rp => rp.Role.RoleName == role 
                                  && !rp.IsDeleted 
                                  && !rp.Role.IsDeleted 
                                  && !rp.Privilege.IsDeleted 
                                  && rp.Menu != null && !rp.Menu.IsDeleted)
                        .Select(rp => $"{rp.Privilege!.PrivilegeName}:{rp.Menu!.MenuName}")
                        .Distinct()
                        .ToListAsync();

                    // Store in cache for 30 seconds
                    cache.Set(cacheKey, rolePermissions, TimeSpan.FromSeconds(30));
                }

                if (rolePermissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase))
                {
                    hasAccess = true;
                    break;
                }
            }

            // 5. Block access if permission is not possessed by any assigned role
            if (!hasAccess)
            {
                var failResponse = ApiResponse<object>.Create(ResponseStatus.Forbidden);
                context.Result = new ObjectResult(failResponse) 
                { 
                    StatusCode = 403 
                };
                return;
            }
        }
    }
}