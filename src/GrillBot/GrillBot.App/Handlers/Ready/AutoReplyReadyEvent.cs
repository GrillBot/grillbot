using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Ready;

public class AutoReplyReadyEvent : IReadyEvent
{
    private AutoReplyManager AutoReplyManager { get; }

    public AutoReplyReadyEvent(AutoReplyManager autoReplyManager)
    {
        AutoReplyManager = autoReplyManager;
    }

    public Task ProcessAsync() => AutoReplyManager.InitAsync();
}
