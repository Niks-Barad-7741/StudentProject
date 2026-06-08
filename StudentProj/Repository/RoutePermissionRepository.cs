using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Repository_Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentProj.Repository
{
    public class RoutePermissionRepository : IRoutePermissionRepository
    {
        private readonly StudentDbcontext _dbcontext;
        // private readonly IMemoryCache _cache;
        private readonly IDistributedCache _cache;
        private const string RoutePermissionsCacheKey = "RoutePermissions_All";

        public RoutePermissionRepository(StudentDbcontext dbcontext, IDistributedCache cache)
        {
            _dbcontext = dbcontext;
            _cache = cache;
        }

        private async Task EvictCacheAsync()
        {
            // _cache.Remove(RoutePermissionsCacheKey);
            await _cache.RemoveAsync(RoutePermissionsCacheKey);
        }

        public async Task<List<RoutePermissions>> GetAllRoutePermissionsAsync()
        {
            return await _dbcontext.RoutePermissions.ToListAsync();
        }

        public async Task<RoutePermissions?> GetRoutePermissionByIdAsync(int id)
        {
            return await _dbcontext.RoutePermissions.FindAsync(id);
        }

        public async Task<RoutePermissions> CreateRoutePermissionAsync(RoutePermissions routePermission)
        {
            await _dbcontext.RoutePermissions.AddAsync(routePermission);
            await _dbcontext.SaveChangesAsync();
            // EvictCache();
            await EvictCacheAsync();
            return routePermission;
        }

        public async Task<bool> UpdateRoutePermissionAsync(int id, RoutePermissions routePermission)
        {
            _dbcontext.RoutePermissions.Update(routePermission);
            var updated = await _dbcontext.SaveChangesAsync();
            if (updated > 0)
            {
                // EvictCache();
                await EvictCacheAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteRoutePermissionAsync(int id)
        {
            var entity = await GetRoutePermissionByIdAsync(id);
            if (entity == null) return false;

            _dbcontext.RoutePermissions.Remove(entity);
            var deleted = await _dbcontext.SaveChangesAsync();
            if (deleted > 0)
            {
                // EvictCache();
                await EvictCacheAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RoutePermissionExistsAsync(string httpMethod, string pathPattern)
        {
            // Normalize path pattern (trim '/' and lower-case) to check duplicate
            string normPath = pathPattern.Trim('/').ToLowerInvariant();
            var existing = await _dbcontext.RoutePermissions.ToListAsync();
            return existing.Any(rp => 
                rp.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase) && 
                rp.PathPattern.Trim('/').ToLowerInvariant() == normPath);
        }
    }
}
