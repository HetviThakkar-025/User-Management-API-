using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Copilot suggested this pattern to catch any unhandled exceptions that
/// slip past controller-level try-catch blocks, preventing raw stack traces
/// from being returned to clients.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception caught by global middleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message    = "An internal server error occurred. Please try again later.",
            // Only expose detail in Development — Copilot recommended this guard
            detail = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                ? exception.Message
                : null
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>Extension method for clean registration in Program.cs.</summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
