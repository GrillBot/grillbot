namespace GrillBot.App.Managers;

public class AuditLogManager
{
    private Dictionary<ulong, DateTime> NextMemberRoleEvents { get; } = new();
    private Dictionary<ulong, DateTime> NextOverwriteEvents { get; } = new();
    private readonly object _locker = new();

    public void OnMemberRoleUpdatedEvent(ulong guildId, DateTime newDate)
    {
        lock (_locker)
        {
            NextMemberRoleEvents.TryAdd(guildId, DateTime.MinValue);
            NextMemberRoleEvents[guildId] = newDate;
        }
    }

    public void OnOverwriteChangedEvent(ulong channelId, DateTime newDate)
    {
        lock (_locker)
        {
            NextOverwriteEvents.TryAdd(channelId, DateTime.MinValue);
            NextOverwriteEvents[channelId] = newDate;
        }
    }

    public DateTime GetNextMemberRoleEvent(ulong guildId)
    {
        lock (_locker)
            return NextMemberRoleEvents.TryGetValue(guildId, out var dateTime) ? dateTime : DateTime.MinValue;
    }

    public DateTime GetNextOverwriteEvent(ulong channelId)
    {
        lock (_locker)
            return NextOverwriteEvents.TryGetValue(channelId, out var dateTime) ? dateTime : DateTime.MinValue;
    }

    public bool CanProcessNextMemberRoleEvent(ulong guildId)
        => DateTime.Now >= GetNextMemberRoleEvent(guildId);

    public bool CanProcessNextOverwriteEvent(ulong channelId)
        => DateTime.Now >= GetNextOverwriteEvent(channelId);
}
