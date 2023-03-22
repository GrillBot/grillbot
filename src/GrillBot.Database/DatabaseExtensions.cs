using GrillBot.Core;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        return services
            .AddDatabaseContext<GrillBotContext>(b => b.UseNpgsql(connectionString))
            .AddSingleton<GrillBotDatabaseBuilder>();
    }
}
