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
        Description = "User Management API for TechHive Solutions – debugged & optimized with Microsoft Copilot"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

// Bug fix: Copilot recommended placing global exception middleware FIRST in the
// pipeline so it wraps every subsequent middleware and catches all unhandled exceptions.
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserManagementAPI v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
