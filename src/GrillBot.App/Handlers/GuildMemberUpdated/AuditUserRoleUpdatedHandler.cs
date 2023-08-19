using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class AuditUserRoleUpdatedHandler : AuditLogServiceHandler, IGuildMemberUpdatedEvent
{
    private AuditLogManager AuditLogManager { get; }
    private ICounterManager CounterManager { get; }

    public AuditUserRoleUpdatedHandler(AuditLogManager auditLogManager, ICounterManager counterManager, IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
        CounterManager = counterManager;
        AuditLogManager = auditLogManager;
    }

    public async Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;
        AuditLogManager.OnMemberRoleUpdatedEvent(after.GuildId, DateTime.Now.AddMinutes(1));

        var auditLogs = await GetAuditLogsAsync(after.Guild, after);
        if (auditLogs.Count == 0)
            return;

        var requests = auditLogs.ConvertAll(entry => CreateRequest(LogType.MemberRoleUpdated, after.Guild, null, after, entry));
        await SendRequestsAsync(requests);
    }

    private bool CanProcess(IGuildUser? before, IGuildUser after)
        => before is not null && DateTime.Now >= AuditLogManager.GetNextMemberRoleEvent(after.GuildId) && !before.RoleIds.OrderBy(o => o).SequenceEqual(after.RoleIds.OrderBy(o => o));

    private async Task<List<IAuditLogEntry>> GetAuditLogsAsync(IGuild guild, IUser user)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.MemberRoleUpdated);
        }

        return auditLogs
            .Where(o => ((MemberRoleAuditLogData)o.Data).Target.Id == user.Id)
            .ToList();
    }
}
