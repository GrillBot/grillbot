using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DiscordHelper
{
    public static DiscordSocketClient CreateClient()
    {
        return new DiscordSocketClient();
    }

    public static IDiscordClient CreateDiscordClient()
    {
        var mock = new Mock<IDiscordClient>();

        var guild = DataHelper.CreateGuild();
        mock.Setup(o => o.CurrentUser).Returns(DataHelper.CreateSelfUser());
        mock.Setup(o => o.GetGuildsAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(new List<IGuild>() { guild }.AsReadOnly() as IReadOnlyCollection<IGuild>));
        mock.Setup(o => o.GetGuildAsync(It.Is<ulong>(o => o == guild.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
            .Returns(Task.FromResult(guild));

        return mock.Object;
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
