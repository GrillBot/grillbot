using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class EmoteChainHandler : IMessageReceivedEvent
{
    private EmoteChainManager EmoteChainManager { get; }

    public EmoteChainHandler(EmoteChainManager emoteChainManager)
    {
        EmoteChainManager = emoteChainManager;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (message.Channel is IDMChannel || !message.Author.IsUser()) return;
        await EmoteChainManager.ProcessChainAsync(message);
    }
}
