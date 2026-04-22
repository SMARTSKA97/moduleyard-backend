using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Modules.Auth;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        // Add other auth related services here
        return services;
    }
}
