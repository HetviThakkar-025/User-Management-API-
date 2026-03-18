using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title       = "UserManagementAPI",
        Version     = "v1",
        Description = "User Management API for TechHive Solutions – with logging, error-handling, and auth middleware"
    });

    // Copilot suggested adding a security definition so Swagger UI shows the
    // Authorize button and passes the Bearer token automatically during testing.
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter your token. Example: hr-secret-token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── Middleware pipeline ───────────────────────────────────────────────────────
var app = builder.Build();

// ┌─────────────────────────────────────────────────────────────────────────┐
// │  MIDDLEWARE ORDER (as specified by TechHive Solutions requirements)     │
// │                                                                         │
// │  1. Error-handling   — outermost, wraps everything, catches all throws  │
// │  2. Authentication   — rejects unauthorized before any logic runs       │
// │  3. Request logging  — logs after auth so only valid requests are logged │
// │  4. Routing / MVC    — actual endpoint handling                         │
// └─────────────────────────────────────────────────────────────────────────┘

// Step 1 – Error handling (must be first so it catches errors from all layers below)
app.UseGlobalExceptionHandler();

// Step 2 – Swagger (exempt from auth; served before authentication middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagementAPI v1");
        c.RoutePrefix = string.Empty;
    });
}

// Step 3 – Token authentication
app.UseTokenAuthentication();

// Step 4 – Request/response logging
app.UseRequestLogging();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

