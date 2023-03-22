﻿using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.GuildMemberUpdated;

public class AuditUserUpdatedHandler : IGuildMemberUpdatedEvent
{
    private ICounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditUserUpdatedHandler(ICounterManager counterManager, AuditLogWriteManager auditLogWriteManager)
    {
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(IGuildUser before, IGuildUser after)
    {
        if (!CanProcess(before, after)) return;

        var auditLog = await FindAuditLogAsync(after.Guild, after);
        if (auditLog == null) return;

        var data = new MemberUpdatedData(before, after);
        var item = new AuditLogDataWrapper(AuditLogItemType.MemberUpdated, data, after.Guild, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriteManager.StoreAsync(item);
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
