using GrillBot.App.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ThreadUpdated;

public class ForumThreadTagsUpdated : IThreadUpdatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ForumThreadTagsUpdated(CounterManager counterManager, AuditLogWriteManager auditLogWriteManager, GrillBotDatabaseBuilder databaseBuilder)
    {
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IThreadChannel? before, ulong threadId, IThreadChannel after)
    {
        if (!CanProcess(before, after)) return;

        var forum = await TryFindForumAsync(after);
        if (forum is null) return;

        var auditLog = await FindAuditLogAsync(after.Guild, after);
        var infoBefore = CreateThreadInfo(forum, before!);
        var infoAfter = CreateThreadInfo(forum, after);
        var diff = new Diff<AuditThreadInfo>(infoBefore, infoAfter);
        var logData = new AuditLogDataWrapper(AuditLogItemType.ThreadUpdated, diff, after.Guild, after, auditLog?.User, auditLog?.Id.ToString(), DateTime.Now);

        await AuditLogWriteManager.StoreAsync(logData);
    }

    private static bool CanProcess(IThreadChannel? before, IThreadChannel after)
    {
        return before is not null && !before.AppliedTags.OrderBy(o => o).SequenceEqual(after.AppliedTags.OrderBy(o => o));
    }

    private static async Task<IForumChannel?> TryFindForumAsync(INestedChannel thread)
    {
        var channel = await thread.Guild.GetChannelAsync(thread.CategoryId!.Value);
        return channel as IForumChannel;
    }

    private static AuditThreadInfo CreateThreadInfo(IForumChannel forum, IThreadChannel thread)
    {
        var info = new AuditThreadInfo(thread);
        info.Tags!.AddRange(thread.AppliedTags.Select(o => forum.Tags.First(x => x.Id == o).Name));

        return info;
    }

    private async Task<IAuditLogEntry?> FindAuditLogAsync(IGuild guild, IGuildChannel thread)
    {
        try
        {
            var ignoredIds = await GetIgnoredAuditLogIds(guild, thread);

            IReadOnlyCollection<IAuditLogEntry> auditLogs;
            using (CounterManager.Create("Discord.API.AuditLog"))
            {
                auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.ThreadUpdate);
            }

            // TODO Limit query only on audit logs with applied tags.
            return auditLogs
                .Where(o => !ignoredIds.Contains(o.Id) && ((ThreadUpdateAuditLogData)o.Data).Thread.Id == thread.Id)
                .MaxBy(o => o.Id);
        }
        catch (NullReferenceException)
        {
            return null; // TODO Remove this catch hack after fix in the discord.net lib.
        }
    }

    private async Task<List<ulong>> GetIgnoredAuditLogIds(IGuild guild, IChannel thread)
    {
        var timeLimit = DateTime.Now.AddMonths(-2);

        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetDiscordAuditLogIdsAsync(guild, thread, new[] { AuditLogItemType.ThreadUpdated }, timeLimit);
    }
}
