using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────
// Copilot suggested registering the service as Singleton so the in-memory list
// persists across requests during development/testing.
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddControllers();

// Swagger / OpenAPI — boilerplate scaffolded with Microsoft Copilot
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title   = "UserManagementAPI",
        Version = "v1",
        Description = "User Management API for TechHive Solutions – built with ASP.NET Core 9"
    });

    // Include XML comments so Swagger shows our <summary> docs
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagementAPI v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
