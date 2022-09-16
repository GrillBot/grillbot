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
        return services
            .AddScoped<Api.V2.GetTodayBirthdayInfo>();
    }
}
