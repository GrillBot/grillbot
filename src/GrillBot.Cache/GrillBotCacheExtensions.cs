using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Core.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Cache;

public static class GrillBotCacheExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSingleton<ProfilePictureManager>()
            .AddSingleton<IMessageCacheManager, MessageCacheManager>()
            .AddScoped<DataCacheManager>()
            .AddRedis(configuration);
    }
}
