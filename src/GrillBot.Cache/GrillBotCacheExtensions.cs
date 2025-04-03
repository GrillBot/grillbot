using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Core;
using GrillBot.Core.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Cache;

public static class GrillBotCacheExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Cache");

        return services
            .AddDatabaseContext<GrillBotCacheContext>(builder => builder.UseNpgsql(connectionString))
            .AddSingleton<GrillBotCacheBuilder>()
            .AddSingleton<ProfilePictureManager>()
            .AddSingleton<IMessageCacheManager, MessageCacheManager>()
            .AddScoped<DataCacheManager>()
            .AddRedis(configuration);
    }

    public static void InitCache(this IApplicationBuilder app)
    {
        app.InitDatabase<GrillBotCacheContext>();

        var builder = app.ApplicationServices.GetRequiredService<GrillBotCacheBuilder>();
        using var repository = builder.CreateRepository();
        repository.MessageIndexRepository.DeleteAllIndexes();
    }
}
