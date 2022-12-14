using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Ready;

public class AutoReplyReadyHandler : IReadyEvent
{
    private AutoReplyManager AutoReplyManager { get; }

    public AutoReplyReadyHandler(AutoReplyManager autoReplyManager)
    {
        AutoReplyManager = autoReplyManager;
    }

    public Task ProcessAsync() => AutoReplyManager.InitAsync();
}
