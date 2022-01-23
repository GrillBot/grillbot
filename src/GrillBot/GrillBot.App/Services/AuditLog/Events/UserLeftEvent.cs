using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class UserLeftEvent : AuditEventBase
{
    private SocketGuild Guild { get; }
    private SocketUser User { get; }

    public UserLeftEvent(AuditLogService auditLogService, SocketGuild guild, SocketUser user) : base(auditLogService)
    {
        Guild = guild;
        User = user;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            User != null &&
            User.Id != Guild.CurrentUser.Id &&
            Guild.CurrentUser.GuildPermissions.ViewAuditLog
        );
    }

    public override async Task ProcessAsync()
    {
        var ban = await Guild.GetBanAsync(User);
        var from = DateTime.UtcNow.AddMinutes(-1);
        RestAuditLogEntry auditLog;
        if (ban != null)
        {
            auditLog = (await Guild.GetAuditLogsAsync(5, actionType: ActionType.Ban).FlattenAsync())
                .FirstOrDefault(o => (o.Data as BanAuditLogData)?.Target.Id == User.Id && o.CreatedAt.DateTime >= from);
        }
        else
        {
            auditLog = (await Guild.GetAuditLogsAsync(5, actionType: ActionType.Kick).FlattenAsync())
                .FirstOrDefault(o => (o.Data as KickAuditLogData)?.Target.Id == User.Id && o.CreatedAt.DateTime >= from);
        }

        var data = new UserLeftGuildData(Guild, User, ban != null, ban?.Reason);
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.UserLeft, Guild, null, auditLog?.User ?? User, jsonData, auditLog?.Id);
    }
}
