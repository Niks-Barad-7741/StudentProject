using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Middleware
{
    public class DynamicRbacMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "UserPermissions_";

        public DynamicRbacMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, StudentDbcontext dbContext)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Check if endpoint requires authorization (has [Authorize] attribute)
            var authorizeAttribute = endpoint.Metadata.GetMetadata<IAuthorizeData>();
            var allowAnonymousAttribute = endpoint.Metadata.GetMetadata<IAllowAnonymous>();

            if (authorizeAttribute == null || allowAnonymousAttribute != null)
            {
                await _next(context);
                return;
            }

            // Ensure user is authenticated
            if (context.User.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, "Unauthorized access.");
                return;
            }

            // Check if Super Admin -> bypass check
            if (context.User.IsInRole("Super Admin"))
            {
                await _next(context);
                return;
            }

            // Resolve endpoint route template
            var routeEndpoint = endpoint as RouteEndpoint;
            string? routeTemplate = routeEndpoint?.RoutePattern?.RawText;

            if (string.IsNullOrEmpty(routeTemplate))
            {
                routeTemplate = context.Request.Path.Value;
            }

            string httpMethod = context.Request.Method;

            // Retrieve RoutePermissions from cache or DB
            var routePermissions = await GetCachedRoutePermissionsAsync(dbContext);

            // Find matching pattern
            var normalizedTemplate = NormalizePath(routeTemplate);
            var match = routePermissions.FirstOrDefault(rp =>
                rp.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase) &&
                NormalizePath(rp.PathPattern) == normalizedTemplate);

            string requiredMenu;
            string requiredPrivilege;

            if (match != null)
            {
                requiredMenu = match.RequiredMenuName;
                requiredPrivilege = match.RequiredPrivilegeName;
            }
            else
            {
                // FALLBACK CONVENTIONS (Option B / Convention-Based)
                // 1. Resolve Privilege Name from HTTP Method
                requiredPrivilege = httpMethod.ToUpperInvariant() switch
                {
                    "GET" => "Read",
                    "POST" => "Create",
                    "PUT" => "Update",
                    "PATCH" => "Update",
                    "DELETE" => "Delete",
                    _ => "Read"
                };

                // 2. Resolve Menu Name from Controller Name
                var controllerName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName;
                if (string.IsNullOrEmpty(controllerName))
                {
                    var segments = normalizedTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    controllerName = segments.LastOrDefault() ?? "Unknown";
                }

                // Normalise controller name: e.g. "Student" -> "Students", "Role" -> "Roles", "Menu" -> "Menus", "Privileges" -> "Privileges"
                // Let's apply standard mapping so conventions work seamlessly with database seeded menu names.
                requiredMenu = NormalizeControllerToMenuName(controllerName);
            }

            // Check if user has permission
            var userIdClaim = context.User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int studentId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, "Invalid user identifier claim.");
                return;
            }

            bool hasAccess = await CheckUserPermissionAsync(dbContext, studentId, requiredMenu, requiredPrivilege);

            if (!hasAccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await WriteErrorResponse(context, StatusCodes.Status403Forbidden, $"Forbidden. Required permission: {requiredPrivilege} on {requiredMenu}.");
                return;
            }

            await _next(context);
        }

        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Trim('/').ToLowerInvariant();
        }

        private string NormalizeControllerToMenuName(string controllerName)
        {
            if (string.IsNullOrEmpty(controllerName)) return "Unknown";
            
            // Standardize pluralization to match DB menu names: e.g., "Student" -> "Students", "Role" -> "Roles"
            if (controllerName.Equals("Student", StringComparison.OrdinalIgnoreCase))
                return "Students";
            if (controllerName.Equals("Role", StringComparison.OrdinalIgnoreCase))
                return "Roles";
            if (controllerName.Equals("Menu", StringComparison.OrdinalIgnoreCase))
                return "Menus";
            if (controllerName.Equals("Privilege", StringComparison.OrdinalIgnoreCase) || controllerName.Equals("Privileges", StringComparison.OrdinalIgnoreCase))
                return "Privileges";

            return controllerName;
        }

        private async Task<List<RoutePermissions>> GetCachedRoutePermissionsAsync(StudentDbcontext dbContext)
        {
            const string RoutePermissionsCacheKey = "RoutePermissions_All";
            if (!_cache.TryGetValue(RoutePermissionsCacheKey, out List<RoutePermissions>? list) || list == null)
            {
                list = await dbContext.RoutePermissions.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(RoutePermissionsCacheKey, list, cacheOptions);
            }
            return list;
        }

        private async Task<bool> CheckUserPermissionAsync(StudentDbcontext dbContext, int studentId, string menuName, string privilegeName)
        {
            string cacheKey = $"{CacheKeyPrefix}{studentId}_{menuName}_{privilegeName}";

            if (_cache.TryGetValue(cacheKey, out bool hasPermission))
            {
                return hasPermission;
            }

            // DB check
            hasPermission = await dbContext.StudentRoles
                .Where(sr => sr.StudentId == studentId && !sr.IsDeleted && !sr.Role.IsDeleted)
                .SelectMany(sr => dbContext.RolePrivileges
                    .Where(rp => rp.RoleId == sr.RoleId 
                        && !rp.IsDeleted 
                        && !rp.Privilege.IsDeleted 
                        && rp.Menu != null && !rp.Menu.IsDeleted)
                    .Select(rp => new { MenuName = rp.Menu!.MenuName, PrivilegeName = rp.Privilege!.PrivilegeName }))
                .AnyAsync(p => p.MenuName.ToLower() == menuName.ToLower() && p.PrivilegeName.ToLower() == privilegeName.ToLower());

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            _cache.Set(cacheKey, hasPermission, cacheOptions);

            return hasPermission;
        }

        private async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                statusCodes = statusCode,
                isSuccess = false,
                message = message,
                response = (object?)null
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
