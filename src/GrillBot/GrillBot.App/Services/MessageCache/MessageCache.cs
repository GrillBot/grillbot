using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.MessageCache;

namespace GrillBot.App.Services.MessageCache;

[Initializable]
public partial class MessageCache : ServiceBase
{
    private ConcurrentDictionary<ulong, CachedMessage> Cache { get; }
    private ConcurrentBag<ulong> InitializedChannels { get; }
    private static SemaphoreSlim IndexLock { get; }

    static MessageCache()
    {
        IndexLock = new SemaphoreSlim(1);
    }

    public MessageCache(DiscordSocketClient client, DiscordInitializationService initializationService,
        GrillBotContextFactory dbFactory) : base(client, dbFactory, initializationService)
    {
        Cache = new ConcurrentDictionary<ulong, CachedMessage>();
        InitializedChannels = new ConcurrentBag<ulong>();

        DiscordClient.MessageDeleted += (message, channel) =>
        {
            TryRemove(message.Id, out var _);
            if (!channel.HasValue) return Task.CompletedTask;

            return AppendAroundAsync(channel.Value, message.Id);
        };

        DiscordClient.MessageReceived += OnMessageReceived;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (!InitializationService.Get()) return;
        if (InitializedChannels.Contains(message.Channel.Id)) return;

        await Task.WhenAll(
            AppendAroundAsync(message.Channel, message.Id),
            DownloadLatestFromChannelAsync(message.Channel)
        );

        InitializedChannels.Add(message.Channel.Id);
    }

    private CachedMessage GetCachedMessage(ulong id) => Cache.TryGetValue(id, out CachedMessage message) ? message : null;

    public IMessage GetMessage(ulong id, bool includeRemoved = false)
    {
        var msg = GetCachedMessage(id);
        if (msg == null || (!includeRemoved && msg.Metadata.State == CachedMessageState.ToBeDeleted))
            return null;

        return msg.Message;
    }

    public bool TryRemove(ulong id, out IMessage message)
    {
        message = null;
        var msg = GetCachedMessage(id);

        if (msg == null)
            return false;

        msg.Metadata.State = CachedMessageState.ToBeDeleted;
        message = msg.Message;
        return true;
    }

    public async Task RunCheckAsync(CancellationToken cancellationToken = default)
    {
        foreach (var (id, msg) in Cache.Where(o => o.Value.Metadata.State != CachedMessageState.None).ToDictionary(o => o.Key, o => o.Value))
        {
            if (msg.Metadata.State == CachedMessageState.ToBeDeleted)
            {
                await RemoveIndexAsync(msg.Message, cancellationToken);
                Cache.Remove(id, out var _);
            }
            else if (msg.Metadata.State == CachedMessageState.NeedsUpdate)
            {
                var channel = msg.Message.Channel;
                var newMessage = await channel.GetMessageAsync(id, options: new() { CancelToken = cancellationToken });

                if (newMessage == null)
                {
                    TryRemove(id, out var _);
                    continue;
                }

                Cache[id] = new CachedMessage(newMessage);
            }
        }

        await RebuildAsync(cancellationToken);
    }

    public void MarkUpdated(ulong messageId)
    {
        var message = GetCachedMessage(messageId);
        if (message == null) return;

        message.Metadata.State = CachedMessageState.NeedsUpdate;
    }

    public int ClearChannel(ulong channelId)
    {
        var toClear = Cache.Where(o => o.Value.Metadata.State != CachedMessageState.ToBeDeleted && o.Value.Message.Channel.Id == channelId).ToList();
        toClear.ForEach(o => TryRemove(o.Key, out var _));

        return toClear.Count;
    }

    public async Task<List<IMessage>> GetMessagesAsync(IChannel channel, IUser author = null, IGuild guild = null, CancellationToken cancellationToken = default)
    {
        var messageIds = await GetMessageIdsAsync(author, channel, guild, cancellationToken);
        if (messageIds.Count == 0) return new();

        return messageIds
            .Select(messageId => Cache[messageId])
            .Where(o => !o.IsDeleted)
            .Select(o => o.Message)
            .ToList();
    }

    public async Task<IMessage> GetLastMessageAsync(IChannel channel = null, IUser author = null, IGuild guild = null, CancellationToken cancellationToken = default)
    {
        var messageIds = await GetMessageIdsAsync(author, channel, guild, cancellationToken);
        if (messageIds.Count == 0) return null;

        return messageIds
            .Select(messageId => Cache[messageId])
            .Where(o => !o.IsDeleted)
            .Select(o => o.Message)
            .OrderByDescending(o => o.Id)
            .FirstOrDefault();
    }
}
