using System.Text.Json;
using LibraryManagement.AppServices.DTOs.Common;

namespace LibraryManagement.API.Middleware;

/// <summary>
/// Turns any unhandled exception into the same JSON envelope the rest of the API uses,
/// so a client never receives an HTML error page.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}.", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                // Too late to replace the response; let the server tear down the connection.
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            // Exception detail is useful locally but must never reach a deployed client.
            var errors = _environment.IsDevelopment()
                ? new[] { ex.GetType().Name, ex.Message }
                : Array.Empty<string>();

            var payload = ApiResponse.Fail("An unexpected error occurred while processing your request.", errors);

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
