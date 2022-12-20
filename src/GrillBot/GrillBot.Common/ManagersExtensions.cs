using GrillBot.Common.Helpers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Cooldown;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Emotes;
using GrillBot.Common.Managers.Events;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common;

public static class ManagersExtensions
{
    public static IServiceCollection AddCommonManagers(this IServiceCollection services)
    {
        return services
            .AddSingleton<InitManager>()
            .AddSingleton<CounterManager>()
            .AddScoped<ApiRequestContext>()
            .AddSingleton<EventLogManager>()
            .AddLoggingServices()
            .AddSingleton<CooldownManager>()
            .AddSingleton<EventManager>();
    }

    public static IServiceCollection AddLocalization(this IServiceCollection services, string basePath, string fileMask)
    {
        var manager = new TextsManager(basePath, fileMask);
        return services.AddSingleton<ITextsManager>(manager);
    }

    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        return services
            .AddSingleton<FormatHelper>()
            .AddSingleton<GuildHelper>();
    }
}
