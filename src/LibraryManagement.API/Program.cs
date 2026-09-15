using System.Text.Json.Serialization;
using LibraryManagement.API.Extensions;
using LibraryManagement.API.Middleware;
using LibraryManagement.AppServices;
using LibraryManagement.AppServices.Settings;
using LibraryManagement.Infrastructure;
using LibraryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is missing from appsettings.json.");

// The API knows only these two entry points into the layers below it.
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddAppServices();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums by name, so responses read as "Admin" rather than 2.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddValidationResponseFormatting();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

// Apply pending migrations on startup, so a fresh clone works with a single run.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management API v1");
    // Serve the UI at the site root so the app opens straight into Swagger.
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Library Management API";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
