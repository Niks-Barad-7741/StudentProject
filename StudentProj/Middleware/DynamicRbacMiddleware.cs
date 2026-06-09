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
// using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StudentProj.Data;
using StudentProj.Models;

namespace StudentProj.Middleware
{
    public class DynamicRbacMiddleware
    {
        private readonly RequestDelegate _next;
        // private readonly IMemoryCache _cache;
        private readonly IDistributedCache _cache;
        private const string CacheKeyPrefix = "UserPermissions_";

        public DynamicRbacMiddleware(RequestDelegate next, IDistributedCache cache)
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
            string requiredPermission;

            if (match != null)
            {
                requiredMenu = match.RequiredMenuName;
                requiredPermission = match.RequiredPermissionName;
            }
            else
            {
                // FALLBACK CONVENTIONS (Option B / Convention-Based)
                // 1. Resolve Permission Name from HTTP Method
                requiredPermission = httpMethod.ToUpperInvariant() switch
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

                // Normalise controller name: e.g. "Student" -> "Students", "Role" -> "Roles", "Menu" -> "Menus", "Permissions" -> "Permissions"
                // Let's apply standard mapping so conventions work seamlessly with database seeded menu names.
                requiredMenu = NormalizeControllerToMenuName(controllerName);
            }

            // Check if user has permission
            bool hasAccess = await CheckUserPermissionAsync(dbContext, context.User, requiredMenu, requiredPermission);

            if (!hasAccess)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await WriteErrorResponse(context, StatusCodes.Status403Forbidden, $"Forbidden. Required permission: {requiredPermission} on {requiredMenu}.");
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
            if (controllerName.Equals("Permission", StringComparison.OrdinalIgnoreCase) || controllerName.Equals("Permissions", StringComparison.OrdinalIgnoreCase))
                return "Permissions";

            return controllerName;
        }

        private async Task<List<RoutePermissions>> GetCachedRoutePermissionsAsync(StudentDbcontext dbContext)
        {
            const string RoutePermissionsCacheKey = "RoutePermissions_All";
            
            /* -- Older Memory Cache Implementation --
            if (!_cache.TryGetValue(RoutePermissionsCacheKey, out List<RoutePermissions>? list) || list == null)
            {
                list = await dbContext.RoutePermissions.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                _cache.Set(RoutePermissionsCacheKey, list, cacheOptions);
            }
            return list;
            ------------------------------------------- */

            // New Redis Caching Implementation
            var cachedJson = await _cache.GetStringAsync(RoutePermissionsCacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<RoutePermissions>>(cachedJson);
                    if (list != null) return list;
                }
                catch
                {
                    // Fallback to database on deserialization error
                }
            }

            var dbList = await dbContext.RoutePermissions.ToListAsync();
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            };
            await _cache.SetStringAsync(RoutePermissionsCacheKey, JsonSerializer.Serialize(dbList), cacheOptions);
            return dbList;
        }

        private async Task<bool> CheckUserPermissionAsync(StudentDbcontext dbContext, ClaimsPrincipal user, string menuName, string permissionName)
        {
            /* -- Older Memory Cache Implementation --
            string cacheKey = $"{CacheKeyPrefix}{studentId}_{menuName}_{permissionName}";
            if (_cache.TryGetValue(cacheKey, out bool hasPermission))
            {
                return hasPermission;
            }
            ------------------------------------------- */

            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            if (!roles.Any())
            {
                return false;
            }

            string requiredPermission = $"{permissionName}:{menuName}";
            bool hasAccess = false;

            foreach (var role in roles)
            {
                string cacheKey = $"Permissions_Role_{role}";

                // New Redis Caching Implementation: Get from Redis
                var cachedJson = await _cache.GetStringAsync(cacheKey);
                List<string>? rolePermissions = null;

                if (!string.IsNullOrEmpty(cachedJson))
                {
                    try
                    {
                        rolePermissions = JsonSerializer.Deserialize<List<string>>(cachedJson);
                    }
                    catch
                    {
                        // Fallback to null on deserialization error
                    }
                }

                if (rolePermissions == null)
                {
                    // Cache Miss: Query SQL Server Database for this role's active permissions
                    rolePermissions = await dbContext.RolePermissions
                        .Where(rp => rp.Role.RoleName == role 
                                  && !rp.IsDeleted 
                                  && !rp.Role.IsDeleted 
                                  && !rp.Permission.IsDeleted 
                                  && rp.Menu != null && !rp.Menu.IsDeleted)
                        .Select(rp => $"{rp.Permission!.PermissionName}:{rp.Menu!.MenuName}")
                        .Distinct()
                        .ToListAsync();

                    // New Redis Caching Implementation: Save to Redis
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                    };
                    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rolePermissions), cacheOptions);
                }

                if (rolePermissions.Any(p => p.Equals(requiredPermission, StringComparison.OrdinalIgnoreCase)))
                {
                    hasAccess = true;
                    break;
                }
            }

            return hasAccess;
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
