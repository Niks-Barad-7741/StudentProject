using Microsoft.AspNetCore.Http;
using StudentProj.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace StudentProj.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILoggingService loggingService)
        {
            await _next(context);

            var path = context.Request.Path.Value ?? string.Empty;

            // Don't auto-log login/register here since we will log them with custom details (success/failure) in the controllers
            if (!path.Equals("/api/Login/Login", StringComparison.OrdinalIgnoreCase) && 
                !path.Equals("/api/Register/Register", StringComparison.OrdinalIgnoreCase))
            {
                var email = context.User.Identity?.IsAuthenticated == true 
                    ? context.User.FindFirst("Email")?.Value ?? context.User.FindFirst(ClaimTypes.Email)?.Value 
                    : "Anonymous";

                var action = $"API Access: {context.Response.StatusCode}";
                await loggingService.LogActivityAsync(email, action, context);
            }
        }
    }
}
