using GrillBot.Common.Extensions;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class AuditOrchestrationHandler
{
    private async Task ProcessRoleChangesAsync(IGuildUser? before, IGuildUser after, CreateItemsPayload payload)
    {
        if (before is null || !_auditLogManager.CanProcessNextMemberRoleEvent(after.Id)) return;
        if (before.RoleIds.IsSequenceEqual(after.RoleIds, o => o)) return;

        _auditLogManager.OnMemberRoleUpdatedEvent(after.Id, DateTime.Now.AddMinutes(1));

        var auditLogs = await ReadAuditLogsAsync<MemberRoleAuditLogData>(after.Guild, ActionType.MemberRoleUpdated);
        var userAuditLogs = auditLogs.FindAll(o => o.data.Target.Id == after.Id);
        if (userAuditLogs.Count == 0) return;

        var guildId = after.Guild.Id.ToString();
        const LogType type = LogType.MemberRoleUpdated;

        foreach (var (entry, _) in userAuditLogs)
            payload.Items.Add(new LogRequest(type, entry.CreatedAt.UtcDateTime, guildId, entry.User.Id.ToString(), null, entry.Id.ToString()));
    }

    private static void ProcessUserChanges(IGuildUser? before, IGuildUser after, CreateItemsPayload payload)
    {
        if (before is null) return;
        if (before.IsDeafened == after.IsDeafened && before.IsMuted == after.IsMuted && before.Nickname == after.Nickname) return;

        payload.Items.Add(new LogRequest(LogType.MemberUpdated, DateTime.UtcNow, after.Guild.Id.ToString())
        {
            MemberUpdated = new MemberUpdatedRequest { UserId = after.Id.ToString() }
        });
    }

}
