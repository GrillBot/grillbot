using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MemberRolesUpdatedEvent : AuditEventBase
{
    private Cacheable<SocketGuildUser, ulong> Before { get; }
    private SocketGuildUser After { get; }
    private DateTime NextEventAt { get; }

    private SocketGuild Guild => After.Guild;
    public bool Finished { get; private set; }

    public MemberRolesUpdatedEvent(AuditLogService auditLogService, Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after,
        DateTime nextEventAt) : base(auditLogService)
    {
        Before = before;
        After = after;
        NextEventAt = nextEventAt;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Before.HasValue &&
            DateTime.Now >= NextEventAt &&
            !Before.Value.Roles.SequenceEqual(After.Roles)
        );
    }

    public override async Task ProcessAsync()
    {
        var timeLimit = DateTime.Now.AddDays(-7);
        var auditLogIds = await AuditLogService.GetDiscordAuditLogIdsAsync(Guild, null, new[] { AuditLogItemType.MemberRoleUpdated }, timeLimit);
        var logs = (await Guild.GetAuditLogsAsync(100, actionType: ActionType.MemberRoleUpdated).FlattenAsync())
            .Where(o => !auditLogIds.Contains(o.Id) && ((MemberRoleAuditLogData)o.Data).Target.Id == After.Id)
            .ToList();

        if (logs.Count == 0)
        {
            Finished = true;
            return;
        }

        var items = new List<AuditLogDataWrapper>();

        foreach (var log in logs)
        {
            var roles = (log.Data as MemberRoleAuditLogData).Roles.Select(o =>
            {
                var role = Guild.GetRole(o.RoleId);
                return role != null ? new AuditRoleUpdateInfo(role, o.Added) : null;
            });

            var logData = new MemberUpdatedData(new AuditUserInfo(After));
            logData.Roles.AddRange(roles);

            items.Add(new AuditLogDataWrapper(AuditLogItemType.MemberRoleUpdated, logData, Guild, processedUser: log.User,
                discordAuditLogItemId: log.Id.ToString(), createdAt: log.CreatedAt.LocalDateTime));
        }

        await AuditLogService.StoreItemsAsync(items);
        Finished = true;
    }
}
