using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Modules.Totp;

public static class TotpModuleExtensions
{
    public static IServiceCollection AddTotpModule(this IServiceCollection services)
    {
        services.AddScoped<ITotpService, TotpService>();
        return services;
    }
}
