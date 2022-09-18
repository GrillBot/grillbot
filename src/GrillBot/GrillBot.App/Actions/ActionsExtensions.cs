using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions;

public static class ActionsExtensions
{
    public static IServiceCollection AddActions(this IServiceCollection services)
    {
        return services
            .AddApiActions();
    }

    private static IServiceCollection AddApiActions(this IServiceCollection services)
    {
        // V1
        // AuditLog
        services
            .AddScoped<Api.V1.AuditLog.RemoveItem>()
            .AddScoped<Api.V1.AuditLog.GetAuditLogList>();

        // V2
        services
            .AddScoped<Api.V2.GetTodayBirthdayInfo>()
            .AddScoped<Api.V2.GetRubbergodUserKarma>();

        return services;
    }
}
