using GrillBot.Database.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<GrillBotContext>(b =>
            {
                b.UseNpgsql(connectionString);
                b.EnableDetailedErrors();
                b.EnableThreadSafetyChecks();
            }, optionsLifetime: ServiceLifetime.Singleton)
            .AddSingleton<GrillBotDatabaseBuilder>();

        return services;
    }

    public static void InitDatabase(this IApplicationBuilder app)
    {
        var builder = app.ApplicationServices.GetRequiredService<GrillBotDatabaseBuilder>();

        using var repository = builder.CreateRepository();
        repository.ProcessMigrations();
    }
}
