using Application.Interfaces;
using Fido2NetLib;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Infrastructure.Modules.Passkey;

public static class PasskeyModuleExtensions
{
    public static IServiceCollection AddPasskeyModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddScoped<IPasskeyCredentialRepository, PasskeyCredentialRepository>();
        services.AddScoped<IPasskeyService, PasskeyService>();
        
        services.AddFido2(options =>
        {
            options.ServerDomain = Environment.GetEnvironmentVariable("FIDO2_SERVER_DOMAIN") ?? "localhost";
            options.ServerName = "ModuleYard";
            options.Origins = new HashSet<string> { Environment.GetEnvironmentVariable("FIDO2_ORIGIN") ?? "http://localhost:4200" };
            options.TimestampDriftTolerance = 300000;
        });

        return services;
    }
}
