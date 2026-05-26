using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace StudentProj.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class HasPrivilegeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public HasPrivilegeAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 1. Verify if the user is logged in
            var user = context.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult(); // Returns 401 Unauthorized
                return;
            }
            if (user.IsInRole("Super Admin"))
            {
                return;
            }

            // 2. Read all "Privilege" claims out of the JWT token
            var userPrivileges = user.Claims
                .Where(c => c.Type.Equals("Privilege", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToList();

            // 3. Check if the user has the required privilege
            if (!userPrivileges.Contains(_permission, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult(); // Short-circuits request & returns 403 Forbidden
                return;
            }
        }
    }
}