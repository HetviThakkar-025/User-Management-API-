using System.Diagnostics;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Logs every incoming HTTP request and its outgoing response for audit purposes.
/// Copilot prompt used: "Generate middleware to log HTTP requests and responses in ASP.NET Core."
/// Copilot suggested using Stopwatch for elapsed time and reading response after awaiting _next.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log incoming request
        _logger.LogInformation(
            "[REQUEST]  {Method} {Path}{QueryString} | IP: {IP}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.Connection.RemoteIpAddress);

        // Pass to next middleware
        await _next(context);

        stopwatch.Stop();

        // Log outgoing response
        _logger.LogInformation(
            "[RESPONSE] {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
