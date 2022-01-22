using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService
{
    public Task<bool> CanProcessUserLeft(SocketGuild guild, SocketUser user)
    {
        return Task.FromResult(
            user != null &&
            user.Id != DiscordClient.CurrentUser.Id &&
            guild.CurrentUser.GuildPermissions.ViewAuditLog
        );
    }

    public async Task ProcessUserLeftAsync(SocketGuild guild, SocketUser user)
    {
        var ban = await guild.GetBanAsync(user);
        var from = DateTime.UtcNow.AddMinutes(-1);
        RestAuditLogEntry auditLog;
        if (ban != null)
        {
            auditLog = (await guild.GetAuditLogsAsync(5, actionType: ActionType.Ban).FlattenAsync())
                .FirstOrDefault(o => (o.Data as BanAuditLogData)?.Target.Id == user.Id && o.CreatedAt.DateTime >= from);
        }
        else
        {
            auditLog = (await guild.GetAuditLogsAsync(5, actionType: ActionType.Kick).FlattenAsync())
                .FirstOrDefault(o => (o.Data as KickAuditLogData)?.Target.Id == user.Id && o.CreatedAt.DateTime >= from);
        }

        var data = new UserLeftGuildData(guild, user, ban != null, ban?.Reason);
        var jsonData = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        await StoreItemAsync(AuditLogItemType.UserLeft, guild, null, auditLog?.User ?? user, jsonData, auditLog?.Id);
    }
}
