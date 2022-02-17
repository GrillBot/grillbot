using GrillBot.Data.Models.MessageCache;

namespace GrillBot.App.Services.MessageCache;

public partial class MessageCache
{
    public async Task AppendAroundAsync(IMessageChannel channel, ulong id, CancellationToken cancellationToken = default)
    {
        if (channel is IDMChannel) return;

        var apiMessages = await channel.GetMessagesAsync(id, Direction.Around, options: new RequestOptions() { CancelToken = cancellationToken }).FlattenAsync();
        var messages = apiMessages
            .Where(m => !Cache.ContainsKey(m.Id))
            .ToList();

        messages.ForEach(m => Cache.TryAdd(m.Id, new CachedMessage(m)));
        await CreateIndexesAsync(messages, cancellationToken);
    }

    public async Task<IMessage> GetMessageAsync(IMessageChannel channel, ulong id, CancellationToken cancellationToken = default)
    {
        var message = GetMessage(id);
        if (message != null) return message;

        message = await channel.GetMessageAsync(id, options: new() { CancelToken = cancellationToken });
        if (message != null)
        {
            await CreateIndexAsync(message, cancellationToken);
            Cache.TryAdd(id, new CachedMessage(message));
        }

        return message;
    }

    public async Task DownloadLatestFromChannelAsync(ISocketMessageChannel channel, CancellationToken cancellationToken = default)
    {
        var messages = (await channel.GetMessagesAsync(options: new() { CancelToken = cancellationToken }).FlattenAsync())
            .Where(o => !Cache.ContainsKey(o.Id))
            .ToList();

        messages.ForEach(o => Cache.TryAdd(o.Id, new CachedMessage(o)));
        await CreateIndexesAsync(messages, cancellationToken);
    }
}
