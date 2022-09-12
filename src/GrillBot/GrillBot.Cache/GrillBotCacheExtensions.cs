using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Managers;
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
            .AddSingleton<MessageCacheManager>()
            .AddSingleton<InviteManager>();
    }

    public static void InitCache(this IApplicationBuilder app)
    {
        var builder = app.ApplicationServices.GetRequiredService<GrillBotCacheBuilder>();

        using var repository = builder.CreateRepository();
        repository.ProcessMigrations();

        var messageIndexes = repository.MessageIndexRepository.GetMessagesAsync().Result;
        if (messageIndexes.Count > 0) repository.RemoveCollection(messageIndexes);

        var expiredDirectApiMessages = repository.DirectApiRepository.FindExpiredMessages();
        if (expiredDirectApiMessages.Count > 0) repository.RemoveCollection(expiredDirectApiMessages);

        var inviteMetadata = repository.InviteMetadataRepository.GetAllInvites();
        if (inviteMetadata.Count > 0) repository.RemoveCollection(inviteMetadata);

        repository.Commit();
    }
}
