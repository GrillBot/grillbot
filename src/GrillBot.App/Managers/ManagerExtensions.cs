using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Managers;

public static class ManagerExtensions
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services
            .AddSingleton<AutoReplyManager>()
            .AddSingleton<EmoteChainManager>()
            .AddSingleton<AuditLogManager>()
            .AddSingleton<AuditLogWriteManager>() // TODO Review and change to scoped.
            .AddSingleton<UserManager>() // TODO Review and change to scoped
            .AddSingleton<UnverifyLogManager>() // TODO review and change to scoped.
            .AddSingleton<UnverifyProfileManager>() // TODO Review and change to scoped.
            .AddSingleton<UnverifyMessageManager>(); // TODO Review and change to scoped.
        
        return services;
    }
}
