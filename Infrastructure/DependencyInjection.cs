using Application.Interfaces;
using Infrastructure.Modules.Auth;
using Infrastructure.Modules.Totp;
using Infrastructure.Modules.Passkey;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistence.Models;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? throw new InvalidOperationException("DATABASE_URL environment variable is missing.");

        // 1. Register the Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Register Modules
        services.AddAuthModule();
        services.AddTotpModule();
        services.AddPasskeyModule(configuration);

        // 3. Register Shared Services & Repositories
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}