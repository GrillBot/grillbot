using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.Unverify
{
    public static class UnverifyExtensions
    {
        public static IServiceCollection AddUnverify(this IServiceCollection services)
        {
            return services
                .AddSingleton<UnverifyChecker>()
                .AddSingleton<UnverifyLogger>()
                .AddSingleton<UnverifyProfileGenerator>()
                .AddSingleton<UnverifyService>();
        }
    }
}
