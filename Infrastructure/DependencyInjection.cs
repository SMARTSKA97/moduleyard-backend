using Application.Interfaces;
using Infrastructure.Persistence.Models;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Repositories;

namespace Infrastructure;

public static class DependencyInjection
{
    // The "this IServiceCollection" makes it an extension method we can call from Program.cs
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // 1. Register the Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Register External Services
        services.AddScoped<ITotpService, TotpService>();

        // 3. Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}