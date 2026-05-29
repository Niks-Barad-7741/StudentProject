using Microsoft.AspNetCore.Http;
using StudentProj.Data;
using StudentProj.Models;
using StudentProj.Common;
using Serilog;
using System;
using System.Threading.Tasks;

namespace StudentProj.Services
{
    public interface ILoggingService
    {
        Task LogActivityAsync(string? name, string? email, string action, HttpContext context);
    }

    public class LoggingService : ILoggingService
    {
        private readonly StudentDbcontext _context;

        public LoggingService(StudentDbcontext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(string? name, string? email, string action, HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;
            var ip = IpHelper.GetClientIpAddress(context);
            var timestamp = DateTime.UtcNow;

            // 1. Log to File using Serilog (Name first, then Email)
            Log.Information("Name: {Name} | User: {Email} | Action: {Action} | Method: {Method} | Path: {Path} | IP: {IP}", 
                name ?? "Anonymous", email ?? "Anonymous", action, method, path, ip);

            // 2. Save to Database Table
            var logEntry = new Logs
            {
                Name = name,
                Email = email,
                Action = action,
                Method = method,
                Path = path,
                IpAddress = ip,
                Timestamp = timestamp
            };

            _context.Logs.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
