using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ThreadDeleted;

public class AuditThreadDeletedHandler : IThreadDeletedEvent
{
    private ChannelHelper ChannelHelper { get; }
    private CounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AuditThreadDeletedHandler(ChannelHelper channelHelper, CounterManager counterManager, AuditLogWriteManager auditLogWriteManager, GrillBotDatabaseBuilder databaseBuilder)
    {
        ChannelHelper = channelHelper;
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IThreadChannel cachedThread, ulong threadId)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(cachedThread, threadId);
        if (guild == null) return;

        var auditLog = await FindAuditLogAsync(guild, threadId);
        if (auditLog == null) return;

        var data = new AuditThreadInfo(auditLog.Data as ThreadDeleteAuditLogData);
        var item = new AuditLogDataWrapper(AuditLogItemType.ThreadDeleted, data, guild, cachedThread, auditLog.User, auditLog.Id.ToString());

        await AuditLogWriteManager.StoreAsync(item);
        await UpdateNonCachedChannelAsync(auditLog, cachedThread, threadId);
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuild guild, ulong threadId)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.ThreadDelete);
        }

        return auditLogs
            .FirstOrDefault(o => ((ThreadDeleteAuditLogData)o.Data).ThreadId == threadId);
    }

    /// <summary>
    /// Correction of the link between the channel and the log entry in the database.
    /// </summary>
    private async Task UpdateNonCachedChannelAsync(IAuditLogEntry logEntry, IGuildChannel cachedThread, ulong threadId)
    {
        if (cachedThread != null) return;

        await using var repository = DatabaseBuilder.CreateRepository();
        var logItem = await repository.AuditLog.FindLogItemByDiscordIdAsync(logEntry.Id, AuditLogItemType.ThreadDeleted);
        if (logItem == null) return;

        logItem.ChannelId = threadId.ToString();
        await repository.CommitAsync();
    }
}
