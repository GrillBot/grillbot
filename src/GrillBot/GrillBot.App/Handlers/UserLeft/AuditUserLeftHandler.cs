using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.UserLeft;

public class AuditUserLeftHandler : IUserLeftEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditUserLeftHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IGuild guild, IUser user)
    {
        if (!await CanProcessAsync(guild, user) || guild is not SocketGuild socketGuild) return;

        var ban = await FindBanAsync(guild, user);
        var auditLog = await FindAuditLogAsync(ban, guild, user);

        var data = new UserLeftGuildData(socketGuild, user, ban != null, ban?.Reason);
        var item = new AuditLogDataWrapper(AuditLogItemType.UserLeft, data, guild, processedUser: auditLog?.User ?? user, discordAuditLogItemId: auditLog?.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }

    private static async Task<bool> CanProcessAsync(IGuild guild, IUser user)
    {
        var currentUser = await guild.GetCurrentUserAsync();
        return user != null && user.Id != currentUser.Id && currentUser.GuildPermissions is { ViewAuditLog: true, BanMembers: true };
    }

    private async Task<IBan> FindBanAsync(IGuild guild, IUser user)
    {
        using (CounterManager.Create("Discord.API.Ban"))
        {
            return await guild.GetBanAsync(user);
        }
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IBan ban, IGuild guild, IUser user)
    {
        var actionType = ban == null ? ActionType.Kick : ActionType.Ban;
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: actionType);
        }

        var timeLimit = DateTime.Now.AddMinutes(-1);
        var query = auditLogs.Where(o => o.CreatedAt.LocalDateTime >= timeLimit);
        return ban != null ? query.FirstOrDefault(o => ((BanAuditLogData)o.Data).Target.Id == user.Id) : query.FirstOrDefault(o => ((KickAuditLogData)o.Data).Target.Id == user.Id);
    }
}
