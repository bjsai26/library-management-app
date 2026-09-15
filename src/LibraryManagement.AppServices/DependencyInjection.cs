using LibraryManagement.AppServices.Interfaces;
using LibraryManagement.AppServices.Security;
using LibraryManagement.AppServices.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.AppServices;

/// <summary>
/// Registers every application service. The API layer calls this and never news up a service.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
