using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Managers;

public static class ManagerExtensions
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services
            .AddSingleton<EmoteChainManager>()
            .AddSingleton<AuditLogManager>()
            .AddSingleton<UserManager>() // TODO Review and change to scoped
            .AddScoped<UnverifyProfileManager>()
            .AddScoped<UnverifyMessageManager>()
            .AddSingleton<PinManager>()
            .AddScoped<DataResolve.DataResolveManager>()
            .AddScoped<Points.PointsManager>()
            .AddScoped<Points.PointsSynchronizationManager>()
            .AddScoped<Points.PointsValidationManager>()
            .AddScoped<LocalizationManager>()
            .AddScoped<Auth.JwtTokenManager>();

        return services;
    }
}
