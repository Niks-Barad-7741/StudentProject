using Microsoft.AspNetCore.Http;
using StudentProj.Data;
using StudentProj.Models;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StudentProj.Services
{
    public interface ILoggingService
    {
        Task LogActivityAsync(string? email, string action, HttpContext context);
    }

    public class LoggingService : ILoggingService
    {
        private readonly StudentDbcontext _context;

        public LoggingService(StudentDbcontext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(string? email, string action, HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var timestamp = DateTime.UtcNow;

            // If it is a local loopback request, resolve the laptop's actual IPv4 address
            if (ip == "::1" || ip == "127.0.0.1")
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var address in host.AddressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ip = address.ToString();
                            break;
                        }
                    }
                }
                catch
                {
                    // Fallback to original ip if resolution fails
                }
            }

            // 1. Log to File using Serilog
            Log.Information("User: {Email} | Action: {Action} | Method: {Method} | Path: {Path} | IP: {IP}", 
                email ?? "Anonymous", action, method, path, ip);

            // 2. Save to Database Table
            var logEntry = new Logs
            {
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
