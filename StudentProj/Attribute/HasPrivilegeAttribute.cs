using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using StudentProj.DTO;
using StudentProj.Enums;
using StudentProj.Repository;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

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

            // 4. Check permissions locally from claims inside the JWT token
            string requiredPermission = $"{_permission}:{_menuName}";
            bool hasAccess = user.Claims.Any(c => 
                c.Type.Equals("Permission", StringComparison.OrdinalIgnoreCase) && 
                c.Value.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase)
            );

            // 5. Block access if permission claim is not found
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