using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class AutoReplyHandler : IMessageReceivedEvent
{
    private AutoReplyManager AutoReplyManager { get; }

    public AutoReplyHandler(AutoReplyManager autoReplyManager)
    {
        AutoReplyManager = autoReplyManager;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!await CanReactAsync(message)) return;

        var match = await AutoReplyManager.FindAsync(message.Content);
        if (match == null) return;
        await message.Channel.SendMessageAsync(match.Reply);
    }

    private async Task<bool> CanReactAsync(IMessage message)
    {
        return message.TryLoadMessage(out var userMessage) && userMessage != null && !userMessage.IsInteractionCommand() && !await AutoReplyManager.IsChannelDisabledAsync(message.Channel.Id);
    }
}
