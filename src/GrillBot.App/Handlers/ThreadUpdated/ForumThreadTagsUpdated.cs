using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ThreadUpdated;

public class ForumThreadTagsUpdated : IThreadUpdatedEvent
{
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public ForumThreadTagsUpdated(IRabbitMQPublisher rabbitPublisher)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(IThreadChannel? before, ulong threadId, IThreadChannel after)
    {
        if (!CanProcess(before, after)) return;

        var forum = await TryFindForumAsync(after);
        if (forum is null) return;

        var guildId = forum.Guild.Id.ToString();
        var channelId = after.Id.ToString();
        var request = new LogRequest(LogType.ThreadUpdated, DateTime.UtcNow, guildId, null, channelId)
        {
            ThreadUpdated = new DiffRequest<ThreadInfoRequest>
            {
                After = CreateThreadInfoRequest(after, forum),
                Before = CreateThreadInfoRequest(before!, forum)
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(request), new());
    }

    private static bool CanProcess(IThreadChannel? before, IThreadChannel after)
        => before?.AppliedTags.OrderBy(o => o).SequenceEqual(after.AppliedTags.OrderBy(o => o)) == false;

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
            ThreadName = thread.Name
        };
    }
}
