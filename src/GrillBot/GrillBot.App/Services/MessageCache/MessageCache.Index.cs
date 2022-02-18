using GrillBot.Database.Entity;

namespace GrillBot.App.Services.MessageCache;

public partial class MessageCache
{
    private Task<List<ulong>> GetMessageIdsFromAuthorAsync(IUser user, CancellationToken cancellationToken = default)
        => GetMessageIdsFromAuthorAsync(user.Id, cancellationToken);

    private async Task<List<ulong>> GetMessageIdsFromAuthorAsync(ulong authorId, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var ids = await context.MessageCacheIndexes.AsNoTracking()
            .Where(o => o.AuthorId == authorId.ToString())
            .ToListAsync(cancellationToken);

        return ids.ConvertAll(o => Convert.ToUInt64(o.MessageId));
    }

    private async Task<List<ulong>> GetMessageIdsFromChannelAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var ids = await context.MessageCacheIndexes.AsNoTracking()
            .Where(o => o.ChannelId == channelId.ToString())
            .ToListAsync(cancellationToken);

        return ids.ConvertAll(o => Convert.ToUInt64(o.MessageId));
    }

    public async Task<int> GetMessagesCountInChannelAsync(ulong channelId, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        return await context.MessageCacheIndexes.CountAsync(o => o.ChannelId == channelId.ToString(), cancellationToken);
    }

    private Task<List<ulong>> GetMessageIdsInChannelFromUserAsync(IUser author, IChannel channel, CancellationToken cancellationToken = default)
        => GetMessageIdsInChannelFromUserAsync(author.Id, channel.Id, cancellationToken);

    private async Task<List<ulong>> GetMessageIdsInChannelFromUserAsync(ulong authorId, ulong channelId, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var ids = await context.MessageCacheIndexes.AsNoTracking()
            .Where(o => o.ChannelId == channelId.ToString() && o.AuthorId == authorId.ToString())
            .ToListAsync(cancellationToken);

        return ids.ConvertAll(o => Convert.ToUInt64(o.MessageId));
    }

    public void ClearIndexes()
    {
        using var context = DbFactory.Create();

        context.Database.ExecuteSqlRaw("TRUNCATE TABLE public.\"MessageCacheIndexes\"");
    }

    private Task RemoveIndexAsync(IMessage message, CancellationToken cancellationToken = default)
        => RemoveIndexAsync(message.Id, cancellationToken);

    private async Task RemoveIndexAsync(ulong messageId, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var entity = await context.MessageCacheIndexes
            .FirstOrDefaultAsync(o => o.MessageId == messageId.ToString(), cancellationToken);
        if (entity == null) return;

        context.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    private Task CreateIndexAsync(IMessage message, CancellationToken cancellationToken = default)
        => CreateIndexAsync(message.Id, message.Channel.Id, message.Author.Id, cancellationToken);

    private async Task CreateIndexAsync(ulong messageId, ulong channelId, ulong authorId, CancellationToken cancellationToken = default)
    {
        var entity = new MessageCacheIndex()
        {
            AuthorId = authorId.ToString(),
            MessageId = messageId.ToString(),
            ChannelId = channelId.ToString(),
        };

        using var context = DbFactory.Create();

        await context.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateIndexesAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        var entities = messages.Select(o => new MessageCacheIndex()
        {
            AuthorId = o.Author.Id.ToString(),
            ChannelId = o.Channel.Id.ToString(),
            MessageId = o.Id.ToString()
        }).ToList();

        using var context = DbFactory.Create();

        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task RebuildAsync(CancellationToken cancellationToken = default)
    {
        var entities = Cache.Values.Select(o => new MessageCacheIndex()
        {
            AuthorId = o.Message.Author.Id.ToString(),
            ChannelId = o.Message.Channel.Id.ToString(),
            MessageId = o.Message.Id.ToString()
        }).ToList();

        if (entities.Count == 0)
            return;

        using var context = DbFactory.Create();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE public.\"MessageCacheIndexes\"", cancellationToken);
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
