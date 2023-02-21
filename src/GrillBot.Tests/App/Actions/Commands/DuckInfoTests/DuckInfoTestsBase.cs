using Discord;
using Discord.WebSocket;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Services;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

public abstract class DuckInfoTestsBase : CommandActionTest<DuckInfo>
{
    protected abstract DuckState? State { get; }

    protected override DuckInfo CreateAction()
    {
        var kachnaOnline = new KachnaOnlineClientBuilder();
        kachnaOnline = State == null ? kachnaOnline.GetCurrentStateWithException() : kachnaOnline.GetCurrentStateWithoutException(State);

        var discordClient = new DiscordSocketClient();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var logging = new LoggingManager(discordClient, interactionService, TestServices.Provider.Value);
        return InitAction(new DuckInfo(kachnaOnline.Build(), TestServices.Texts.Value, TestServices.Configuration.Value, logging));
    }

    public abstract Task RunTestAsync();

    protected static void CheckEmbed(Embed? embed)
    {
        Assert.IsNotNull(embed);
        Assert.IsNotNull(embed.Author);
        Assert.IsNotNull(embed.Color);
        Assert.IsNotNull(embed.Timestamp);
        Assert.IsFalse(string.IsNullOrEmpty(embed.Title));
    }
}
