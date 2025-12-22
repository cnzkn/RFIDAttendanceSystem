using CloudProject.Database.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CloudProject.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabase(this IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var user = Environment.GetEnvironmentVariable("DB_USER");
            var pass = Environment.GetEnvironmentVariable("DB_PASS");
            var name = Environment.GetEnvironmentVariable("DB_NAME");
            var ssl = Environment.GetEnvironmentVariable("DB_SECURE") ?? "Require";

            options.UseNpgsql($"Host={host};Port={port};Username={user};Password={pass};Database={name};SslMode={ssl};Trust Server Certificate=true;");
            options.UseLazyLoadingProxies();
            options.ConfigureWarnings(x => x.Log(CoreEventId.ManyServiceProvidersCreatedWarning));
        });
        
        services.AddScoped<DatabaseSeeder>();

        services.AddScoped(typeof(IRepository<>), typeof(RelationalRepository<>));

        return services;
    }

    public static IdentityBuilder AddApplicationIdentity(this IServiceCollection services)
    {
        return services.AddIdentityCore<UserModel>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<DatabaseContext>();
    }
}
