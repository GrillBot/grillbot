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
            if (!NextMemberRoleEvents.ContainsKey(guildId))
                NextMemberRoleEvents.Add(guildId, DateTime.MinValue);

            NextMemberRoleEvents[guildId] = newDate;
        }
    }

    public void OnOverwriteChangedEvent(ulong channelId, DateTime newDate)
    {
        lock (_locker)
        {
            if (!NextOverwriteEvents.ContainsKey(channelId))
                NextOverwriteEvents.Add(channelId, DateTime.MinValue);

            NextOverwriteEvents[channelId] = newDate;
        }
    }

    public DateTime GetNextMemberRoleEvent(ulong guildId)
    {
        lock (_locker)
        {
            return NextMemberRoleEvents.TryGetValue(guildId, out var dateTime) ? dateTime : DateTime.MinValue;
        }
    }

    public DateTime GetNextOverwriteEvent(ulong channelId)
    {
        lock (_locker)
        {
            return NextOverwriteEvents.TryGetValue(channelId, out var dateTime) ? dateTime : DateTime.MinValue;
        }
    }
}
