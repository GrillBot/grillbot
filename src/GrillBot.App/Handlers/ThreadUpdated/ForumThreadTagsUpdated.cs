using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers.ThreadUpdated;

public class ForumThreadTagsUpdated : AuditLogServiceHandler, IThreadUpdatedEvent
{
    public ForumThreadTagsUpdated(IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
    }

    public async Task ProcessAsync(IThreadChannel? before, ulong threadId, IThreadChannel after)
    {
        if (!CanProcess(before, after)) return;

        var forum = await TryFindForumAsync(after);
        if (forum is null) return;

        var request = CreateRequest(LogType.ThreadUpdated, forum.Guild, after);
        request.ThreadUpdated = new DiffRequest<ThreadInfoRequest>
        {
            After = CreateThreadInfoRequest(after, forum),
            Before = CreateThreadInfoRequest(before!, forum)
        };

        await SendRequestAsync(request);
    }

    private static bool CanProcess(IThreadChannel? before, IThreadChannel after)
        => before is not null && !before.AppliedTags.OrderBy(o => o).SequenceEqual(after.AppliedTags.OrderBy(o => o));

    private static async Task<IForumChannel?> TryFindForumAsync(INestedChannel thread)
    {
        var channel = await thread.Guild.GetChannelAsync(thread.CategoryId!.Value);
        return channel as IForumChannel;
    }

    private static ThreadInfoRequest CreateThreadInfoRequest(IThreadChannel thread, IForumChannel forum)
    {
        return new ThreadInfoRequest
        {
            Tags = thread.AppliedTags.Select(o => forum.Tags.First(x => x.Id == o).Name).ToList(),
            Type = thread.Type,
            ArchiveDuration = (int)thread.AutoArchiveDuration,
            IsArchived = thread.IsArchived,
            IsLocked = thread.IsLocked,
            SlowMode = thread.SlowModeInterval,
            ThreadName = thread.Name,
            ParentChannelId = thread.CategoryId!.ToString()!
        };
    }
}
