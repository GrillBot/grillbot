using GrillBot.App.Managers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class AuditUserRoleUpdatedHandler : IGuildMemberUpdatedEvent
{
    private AuditLogManager AuditLogManager { get; }
    private CounterManager CounterManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditUserRoleUpdatedHandler(AuditLogManager auditLogManager, CounterManager counterManager, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriter auditLogWriter)
    {
        DatabaseBuilder = databaseBuilder;
        CounterManager = counterManager;
        AuditLogManager = auditLogManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IGuildUser before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        var ignoredLogIdsAsync = await GetIgnoredAuditLogIdsAsync(after.Guild);
        var auditLogs = await GetAuditLogsAsync(after.Guild, ignoredLogIdsAsync, after);

        if (auditLogs.Count == 0)
        {
            AuditLogManager.UpdateMemberRoleDateAsync(after.GuildId, DateTime.Now.AddMinutes(1));
            return;
        }

        var items = new List<AuditLogDataWrapper>();
        foreach (var log in auditLogs)
        {
            var roles = ((MemberRoleAuditLogData)log.Data).Roles.Select(o =>
            {
                var role = after.Guild.GetRole(o.RoleId);
                return role != null ? new AuditRoleUpdateInfo(role, o.Added) : null;
            });

            var logData = new MemberUpdatedData(new AuditUserInfo(after));
            logData.Roles.AddRange(roles);

            items.Add(new AuditLogDataWrapper(AuditLogItemType.MemberRoleUpdated, logData, after.Guild, processedUser: log.User,
                discordAuditLogItemId: log.Id.ToString(), createdAt: log.CreatedAt.LocalDateTime));
        }

        await AuditLogWriter.StoreAsync(items);
        AuditLogManager.UpdateMemberRoleDateAsync(after.GuildId, DateTime.Now.AddMinutes(1));
    }

    private bool CanProcess(IGuildUser before, IGuildUser after)
        => before != null && DateTime.Now >= AuditLogManager.GetNextMemberRoleDateTime(after.GuildId) && !before.RoleIds.SequenceEqual(after.RoleIds);

    private async Task<List<ulong>> GetIgnoredAuditLogIdsAsync(IGuild guild)
    {
        var timeLimit = DateTime.Now.AddMonths(-2);

        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetDiscordAuditLogIdsAsync(guild, null, new[] { AuditLogItemType.MemberRoleUpdated }, timeLimit);
    }

    private async Task<List<IAuditLogEntry>> GetAuditLogsAsync(IGuild guild, List<ulong> ignoredLogIds, IUser user)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.MemberRoleUpdated);
        }

        return auditLogs
            .Where(o => !ignoredLogIds.Contains(o.Id) && ((MemberRoleAuditLogData)o.Data).Target.Id == user.Id)
            .ToList();
    }
}
