using System.Text;
using LibraryManagement.AppServices.DTOs.Common;
using LibraryManagement.AppServices.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace LibraryManagement.API.Extensions;

/// <summary>
/// Presentation-layer setup: authentication, Swagger, and the shape of automatic
/// model-validation failures.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("The Jwt section is missing from appsettings.json.");

        if (string.IsNullOrWhiteSpace(settings.Key) || settings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be at least 32 characters for HMAC-SHA256 signing.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    // No grace period: an expired token is rejected the moment it expires.
                    ClockSkew = TimeSpan.Zero
                };

                // Return the standard JSON envelope instead of an empty 401/403 body.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(
                            ApiResponse.Fail("Authentication is required. Provide a valid Bearer token."));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(
                            ApiResponse.Fail("Your role does not permit this action."));
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Replaces the default ValidationProblemDetails body, so DataAnnotation failures come
    /// back in the same envelope as every other response.
    /// </summary>
    public static IServiceCollection AddValidationResponseFormatting(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        string.IsNullOrWhiteSpace(entry.Key)
                            ? error.ErrorMessage
                            : $"{entry.Key}: {error.ErrorMessage}"))
                    .ToList();

                return new BadRequestObjectResult(
                    ApiResponse.Fail("One or more validation errors occurred.", errors));
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Library Management API",
                Version = "v1",
                Description =
                    "**To authenticate:** call `POST /api/auth/login`, copy the `accessToken` " +
                    "from the response, click **Authorize** above and paste it.\n\n" +
                    "Seeded accounts: `admin@library.com` / `Admin@123` (full access) and " +
                    "`member@library.com` / `Member@123` (read only)."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste the access token only. The Bearer prefix is added automatically.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // Applies the scheme to every operation, so the Authorize button drives them all.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
            });

            // Surface the XML doc comments written on the controllers.
            var xmlPath = Path.Combine(
                AppContext.BaseDirectory,
                $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
