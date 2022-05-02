using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.App;
using GrillBot.App.Infrastructure.TypeReaders;

namespace GrillBot.Tests.TestHelpers;

public static class DiscordHelper
{
    public static DiscordSocketClient CreateClient()
    {
        return new DiscordSocketClient();
    }

    public static CommandService CreateCommandsService(bool init = false)
    {
        var service = new CommandService();

        if (init)
        {
            var provider = DIHelper.CreateInitializedProvider();

            service.RegisterTypeReaders();
            service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();
        }

        return service;
    }

    public static InteractionService CreateInteractionService(DiscordSocketClient discordClient, bool init = false)
    {
        var service = new InteractionService(discordClient);

        if (init)
        {
            var provider = DIHelper.CreateInitializedProvider();

            service.RegisterTypeConverters();
            service.AddModulesAsync(typeof(Startup).Assembly, provider).Wait();
        }

        return service;
    }
}
