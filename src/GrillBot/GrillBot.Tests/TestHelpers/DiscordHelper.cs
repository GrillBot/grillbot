using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Moq;
using Moq.Protected;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DiscordHelper
{
    public static DiscordSocketClient CreateClient()
    {
        return new DiscordSocketClient();
    }

    public static CommandService CreateCommandsService()
    {
        return new CommandService();
    }

    public static InteractionService CreateInteractionService(DiscordSocketClient discordClient = null)
    {
        return new InteractionService(discordClient ?? CreateClient());
    }
}
