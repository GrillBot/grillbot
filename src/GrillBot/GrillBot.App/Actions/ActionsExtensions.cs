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
            .AddScoped<Api.V1.AuditLog.GetAuditLogList>()
            .AddScoped<Api.V1.AuditLog.GetFileContent>()
            .AddScoped<Api.V1.AuditLog.CreateLogItem>();

        // Auth
        services
            .AddScoped<Api.V1.Auth.GetRedirectLink>()
            .AddScoped<Api.V1.Auth.ProcessCallback>()
            .AddScoped<Api.V1.Auth.CreateToken>();
        
        // AutoReply
        services
            .AddScoped<Api.V1.AutoReply.GetAutoReplyList>()
            .AddScoped<Api.V1.AutoReply.GetAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.CreateAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.UpdateAutoReplyItem>()
            .AddScoped<Api.V1.AutoReply.RemoveAutoReplyItem>();
        
        // Channel
        services
            .AddScoped<Api.V1.Channel.GetChannelUsers>()
            .AddScoped<Api.V1.Channel.SendMessageToChannel>()
            .AddScoped<Api.V1.Channel.GetChannelList>()
            .AddScoped<Api.V1.Channel.ClearMessageCache>()
            .AddScoped<Api.V1.Channel.GetChannelDetail>();

        // V2
        services
            .AddScoped<Api.V2.GetTodayBirthdayInfo>()
            .AddScoped<Api.V2.GetRubbergodUserKarma>();

        return services;
    }
}
