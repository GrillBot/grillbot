using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.AuditLog.Events;

public class ThreadDeletedEvent : AuditEventBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private Cacheable<SocketThreadChannel, ulong> CachedThread { get; }

    public ThreadDeletedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, Cacheable<SocketThreadChannel, ulong> cachedThread,
        IServiceProvider serviceProvider) : base(auditLogService, auditLogWriter)
    {
        CachedThread = cachedThread;
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    public override Task<bool> CanProcessAsync()
        => Task.FromResult(true);

    public override async Task ProcessAsync()
    {
        var guild = await AuditLogService.GetGuildFromChannelAsync(null, CachedThread.Id);
        if (guild == null) return;

        var auditLog = (await guild.GetAuditLogsAsync(actionType: ActionType.ThreadDelete))
            .FirstOrDefault(o => CachedThread.Id == ((ThreadDeleteAuditLogData)o.Data).ThreadId);
        if (auditLog == null) return;

        var data = new AuditThreadInfo(auditLog.Data as ThreadDeleteAuditLogData);
        var item = new AuditLogDataWrapper(AuditLogItemType.ThreadDeleted, data, guild, CachedThread.Value, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
        await UpdateNonCachedChannelAsync(auditLog);
    }

    private async Task UpdateNonCachedChannelAsync(IAuditLogEntry auditLog)
    {
        if (CachedThread.HasValue) return;

        await using var repository = DatabaseBuilder.CreateRepository();
        var logItem = await repository.AuditLog.FindLogItemByDiscordIdAsync(auditLog.Id, AuditLogItemType.ThreadDeleted);
        if (logItem == null) return;

        logItem.ChannelId = CachedThread.Id.ToString();
        await repository.CommitAsync();
    }
}
