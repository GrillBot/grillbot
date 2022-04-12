using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace GrillBot.Database;

static public class DatabaseExtensions
{
    static public IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<GrillBotContext>(b =>
            {
                b.UseNpgsql(connectionString);
                b.EnableDetailedErrors();
            }, optionsLifetime: ServiceLifetime.Singleton)
            .AddSingleton<GrillBotContextFactory>();

        return services;
    }
}
