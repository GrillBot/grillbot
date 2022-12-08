namespace GrillBot.App.Managers;

public class AuditLogManager
{
    private Dictionary<ulong, DateTime> NextMemberRoleEvents { get; } = new();
    private readonly object _locker = new();

    public void UpdateMemberRoleDateAsync(ulong guildId, DateTime newDate)
    {
        lock (_locker)
        {
            if (!NextMemberRoleEvents.ContainsKey(guildId))
                NextMemberRoleEvents.Add(guildId, DateTime.MinValue);

            NextMemberRoleEvents[guildId] = newDate;
        }
    }

    public DateTime GetNextMemberRoleDateTime(ulong guildId)
    {
        lock (_locker)
        {
            return NextMemberRoleEvents.TryGetValue(guildId, out var dateTime) ? dateTime : DateTime.MinValue;
        }
    }
}
