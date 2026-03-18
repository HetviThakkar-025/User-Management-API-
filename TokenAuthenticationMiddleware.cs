using System.Text.Json;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Token-based authentication middleware.
///
/// Copilot prompt used: "Write middleware that validates bearer tokens from incoming
/// requests in ASP.NET Core and returns 401 Unauthorized for invalid tokens."
///
/// Copilot recommendations applied:
///  - Read token from the Authorization header (Bearer scheme)
///  - Skip auth for Swagger UI and health-check paths
///  - Return 401 with a consistent JSON body on failure
///  - Store valid token claims on HttpContext.Items for downstream use
///  - Load valid tokens from configuration so they're not hardcoded
/// </summary>
public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private readonly IConfiguration _configuration;

    // Paths that are exempt from authentication
    private static readonly string[] PublicPaths =
    [
        "/swagger",
        "/favicon.ico",
        "/health"
    ];

    public TokenAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<TokenAuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next          = next;
        _logger        = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Allow public paths through without a token
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // Extract Bearer token from Authorization header
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[AUTH] Missing or malformed Authorization header on {Method} {Path}",
                context.Request.Method, path);

            await WriteUnauthorizedAsync(context, "Authorization header is missing or invalid. Use: Bearer <token>");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();

        if (!IsValidToken(token))
        {
            _logger.LogWarning("[AUTH] Invalid token on {Method} {Path}", context.Request.Method, path);
            await WriteUnauthorizedAsync(context, "Invalid or expired token.");
            return;
        }

        // Copilot suggested storing decoded identity info in HttpContext.Items
        // so downstream controllers can access it without re-validating
        context.Items["AuthenticatedUser"] = ResolveUser(token);
        _logger.LogInformation("[AUTH] Token validated for {Method} {Path}", context.Request.Method, path);

        await _next(context);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool IsPublicPath(string path) =>
        PublicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Validates the token against the configured list of valid tokens.
    /// In a real application this would verify a JWT signature/expiry.
    /// Copilot suggested reading tokens from config to avoid hardcoding secrets.
    /// </summary>
    private bool IsValidToken(string token)
    {
        var validTokens = _configuration
            .GetSection("Authentication:ValidTokens")
            .Get<string[]>() ?? [];

        return validTokens.Contains(token, StringComparer.Ordinal);
    }

    /// <summary>
    /// Returns a display name for the token holder.
    /// In a real JWT implementation Copilot suggested extracting Claims here.
    /// </summary>
    private static string ResolveUser(string token) =>
        token switch
        {
            "hr-secret-token"  => "HR Department",
            "it-secret-token"  => "IT Department",
            "admin-token-2024" => "Administrator",
            _                  => "Authenticated User"
        };

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;

        var body = new
        {
            error      = "Unauthorized.",
            statusCode = 401,
            message
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, options));
    }
}

public static class TokenAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder app)
        => app.UseMiddleware<TokenAuthenticationMiddleware>();
}
