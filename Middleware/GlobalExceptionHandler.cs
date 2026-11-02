using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MovieTicketBooking.Middleware
{
    // Centralized middleware for catching unhandled exceptions throughout the application pipeline.
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException           => HttpStatusCode.BadRequest,
                KeyNotFoundException        => HttpStatusCode.NotFound,
                InvalidOperationException   => HttpStatusCode.Conflict,
                _                           => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = statusCode switch
                {
                    HttpStatusCode.Unauthorized    => "Unauthorized",
                    HttpStatusCode.BadRequest      => "Bad Request",
                    HttpStatusCode.NotFound        => "Not Found",
                    HttpStatusCode.Conflict        => "Conflict",
                    _                              => "Internal Server Error"
                },
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            var json = JsonSerializer.Serialize(problemDetails);
            await context.Response.WriteAsync(json);
        }
    }
}
