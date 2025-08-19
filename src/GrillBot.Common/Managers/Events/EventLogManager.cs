namespace GrillBot.Common.Managers.Events;

public class EventLogManager
{
    private readonly object _locker = new();
    private readonly Dictionary<string, long> _statistics = [];

    public Action<string, long>? OnEventReceived { get; set; }

    public Task ThreadUpdated() => AddToLog(nameof(ThreadUpdated));
    public Task GuildAvailable() => AddToLog(nameof(GuildAvailable));
    public Task JoinedGuild() => AddToLog(nameof(JoinedGuild));
    public Task InviteCreated() => AddToLog(nameof(InviteCreated));
    public Task ThreadDeleted() => AddToLog(nameof(ThreadDeleted));
    public Task GuildMemberUpdated() => AddToLog(nameof(GuildMemberUpdated));
    public Task GuildUpdated() => AddToLog(nameof(GuildUpdated));
    public Task ChannelUpdated() => AddToLog(nameof(ChannelUpdated));
    public Task ChannelDeleted() => AddToLog(nameof(ChannelDeleted));
    public Task ChannelCreated() => AddToLog(nameof(ChannelCreated));
    public Task MessageDeleted() => AddToLog(nameof(MessageDeleted));
    public Task MessageUpdated() => AddToLog(nameof(MessageUpdated));
    public Task UserUnbanned() => AddToLog(nameof(UserUnbanned));
    public Task Ready() => AddToLog(nameof(Ready));
    public Task UserJoined() => AddToLog(nameof(UserJoined));
    public Task UserLeft() => AddToLog(nameof(UserLeft));
    public Task UserUpdated() => AddToLog(nameof(UserUpdated));
    public Task Reaction() => AddToLog(nameof(Reaction));
    public Task MessageReceived() => AddToLog(nameof(MessageReceived));
    public Task InteractionExecuted() => AddToLog(nameof(InteractionExecuted));
    public Task RoleDeleted() => AddToLog(nameof(RoleDeleted));

    private Task AddToLog(string method)
    {
        lock (_locker)
        {
            if (!_statistics.ContainsKey(method))
                _statistics.Add(method, 0);
            _statistics[method]++;
            OnEventReceived?.Invoke(method, _statistics[method]);
        }

        return Task.CompletedTask;
    }
}
