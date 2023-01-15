using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.App;
using GrillBot.App.Infrastructure.TypeReaders;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DiscordHelper
{
    public static CommandService CreateCommandsService(IServiceProvider? provider = null)
    {
        var service = new CommandService();

        if (provider == null)
            return service;

        service.RegisterTypeReaders();
        service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();

        return service;
    }

    public static InteractionService CreateInteractionService(DiscordSocketClient discordClient, IServiceProvider? provider = null)
    {
        var service = new InteractionService(discordClient);

        if (provider == null)
            return service;

        service.RegisterTypeConverters();
        service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();
        return service;
    }
}
