using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class AuditUserUpdatedHandler : IGuildMemberUpdatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditUserUpdatedHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IGuildUser before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        var auditLog = await FindAuditLogAsync(after.Guild, after);
        if (auditLog == null) return;

        var data = new MemberUpdatedData(before, after);
        var item = new AuditLogDataWrapper(AuditLogItemType.MemberUpdated, data, after.Guild, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }

    private static bool CanProcess(IGuildUser before, IGuildUser after)
        => before != null && (before.IsDeafened != after.IsDeafened || before.IsMuted != after.IsMuted || before.Nickname != after.Nickname);

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuild guild, IUser user)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.MemberUpdated);
        }

        return auditLogs
            .FirstOrDefault(o => ((MemberUpdateAuditLogData)o.Data).Target.Id == user.Id);
    }
}
