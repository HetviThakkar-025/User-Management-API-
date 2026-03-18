using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Global error-handling middleware — MUST be first in the pipeline.
///
/// Copilot prompt used: "Create middleware that catches unhandled exceptions and returns
/// consistent JSON error responses in ASP.NET Core."
///
/// Copilot recommended:
///  - Always setting Content-Type to application/json before writing the body
///  - Hiding stack traces in Production to avoid leaking implementation details
///  - Using a consistent envelope shape { error, statusCode, detail } across all failures
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var body = new
        {
            error      = "Internal server error.",
            statusCode = context.Response.StatusCode,
            // Copilot suggested only surfacing detail in Development
            detail = _env.IsDevelopment() ? exception.Message : null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, options));
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
