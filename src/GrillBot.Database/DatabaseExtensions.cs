using GrillBot.Core;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Database;

public static class DatabaseExtensions
{
    private static string SQL_RESTART_ERROR_CODE = "57P01";

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        return services
            .AddDatabaseContext<GrillBotContext>(
                b => b.UseNpgsql(
                    connectionString,
                    opt => opt.EnableRetryOnFailure([SQL_RESTART_ERROR_CODE])
                )
            )
            .AddSingleton<GrillBotDatabaseBuilder>();
    }
}
