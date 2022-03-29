using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class UserUnbannedEvent : AuditEventBase
{
    private SocketGuild Guild { get; }
    private SocketUser User { get; }

    public UserUnbannedEvent(AuditLogService auditLogService, SocketGuild guild, SocketUser user) : base(auditLogService)
    {
        Guild = guild;
        User = user;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(true);

    public override async Task ProcessAsync()
    {
        var auditLog = (await Guild.GetAuditLogsAsync(10, actionType: ActionType.Unban).FlattenAsync())
            .FirstOrDefault(o => ((UnbanAuditLogData)o.Data).Target.Id == User.Id);
        if (auditLog == null) return;

        var data = new AuditUserInfo(User);
        var item = new AuditLogDataWrapper(AuditLogItemType.Unban, data, Guild, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogService.StoreItemAsync(item);
    }
}
