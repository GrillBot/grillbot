using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
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
            .AddDbContext<GrillBotCacheContext>(opt => opt.EnableDetailedErrors().EnableThreadSafetyChecks().UseNpgsql(connectionString), ServiceLifetime.Scoped, ServiceLifetime.Singleton)
            .AddSingleton<GrillBotCacheBuilder>()
            .AddSingleton<ProfilePictureManager>()
            .AddSingleton<MessageCacheManager>();
    }

    public static void InitCache(this IApplicationBuilder app)
    {
        var builder = app.ApplicationServices.GetRequiredService<GrillBotCacheBuilder>();

        using var repository = builder.CreateRepository();
        repository.ProcessMigrations();

        var messageIndexes = repository.MessageIndexRepository.GetMessagesAsync().Result;
        repository.RemoveCollection(messageIndexes);

        var expiredDirectApiMessages = repository.DirectApiRepository.FindExpiredMessagesAsync().Result;
        repository.RemoveCollection(expiredDirectApiMessages);

        repository.Commit();
    }
}
