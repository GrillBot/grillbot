using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Database
{
    static public class DatabaseExtensions
    {
        static public IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services
                .AddDbContext<GrillBotContext>(b => b.UseNpgsql(connectionString), optionsLifetime: ServiceLifetime.Singleton)
                .AddSingleton<GrillBotContextFactory>();

            return services;
        }
    }
}
