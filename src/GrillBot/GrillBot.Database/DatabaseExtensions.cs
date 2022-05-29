using GrillBot.Database.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Database;

static public class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<GrillBotContext>(b =>
            {
                b.UseNpgsql(connectionString);
                b.EnableDetailedErrors();
            }, optionsLifetime: ServiceLifetime.Singleton)
            .AddSingleton<GrillBotDatabaseFactory>();

        return services;
    }

    public static void InitDatabase(this IApplicationBuilder app)
    {
        var builder = app.ApplicationServices.GetRequiredService<GrillBotDatabaseFactory>();

        using var repository = builder.CreateRepository();
        repository.ProcessMigrations();
    }
}
