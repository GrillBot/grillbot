using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.UserUnbanned;

public class AuditUserUnbannedHandler : IUserUnbannedEvent
{
    private ICounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditUserUnbannedHandler(ICounterManager counterManager, AuditLogWriteManager auditLogWriteManager)
    {
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(IUser user, IGuild guild)
    {
        var auditLog = await FindAuditLogAsync(guild, user);
        if (auditLog == null) return;

        var data = new AuditUserInfo(user);
        var item = new AuditLogDataWrapper(AuditLogItemType.Unban, data, guild, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriteManager.StoreAsync(item);
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuild guild, IUser user)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.Unban);
        }

        return auditLogs
            .FirstOrDefault(o => ((UnbanAuditLogData)o.Data).Target.Id == user.Id);
    }
}
