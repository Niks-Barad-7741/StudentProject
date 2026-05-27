using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using StudentProj.Enums;

namespace StudentProj.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Let the request proceed to controllers/repositories
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the complete error trace in the server console/logs
                _logger.LogError(ex, "An unhandled exception occurred during the request.");

                // Handle and serialize the exception into a clean JSON response
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // 1. Assign correct HTTP status codes depending on the type of error thrown
            var status = exception switch
            {
                ArgumentException => Enums.ResponseStatus.BadRequest,       // 400 Bad Request
                KeyNotFoundException => Enums.ResponseStatus.NotFound,      // 404 Not Found
                UnauthorizedAccessException => Enums.ResponseStatus.Unauthorized, // 401 Unauthorized
                _ => Enums.ResponseStatus.InternalServerError               // 500 Server Error
            };

            context.Response.StatusCode = (int)status;

            // 2. Format a clean JSON object to send back to the client
            var errorResponse = new
            {
                status = (int)status,
                message = status.ToFriendlyMessage()
            };

            /* Original format commented out:
            var errorResponse = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Details = exception.InnerException?.Message
            };
            */

            var jsonResult = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResult);
        }
    }
}