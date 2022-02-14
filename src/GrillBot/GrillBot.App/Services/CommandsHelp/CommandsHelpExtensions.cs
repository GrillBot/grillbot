using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.CommandsHelp;

public static class CommandsHelpExtensions
{
    public static IServiceCollection AddCommandsHelp(this IServiceCollection services)
    {
        return services
            .AddSingleton<CommandsHelpService>()
            .AddSingleton<ExternalCommandsHelpService>();
    }
}
