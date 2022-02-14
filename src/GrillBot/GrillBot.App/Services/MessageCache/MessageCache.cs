using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.MessageCache;
using System.Collections.Concurrent;

namespace GrillBot.App.Services.MessageCache
{
    public class MessageCache : ServiceBase
    {
        private ConcurrentDictionary<ulong, CachedMessage> Cache { get; }
        private ConcurrentBag<ulong> InitializedChannels { get; }

        public MessageCache(DiscordSocketClient client, DiscordInitializationService initializationService) : base(client, null, initializationService)
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

        public async Task DownloadLatestFromChannelAsync(ISocketMessageChannel channel)
        {
            var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
            messages.ForEach(o => Cache.TryAdd(o.Id, new CachedMessage(o)));
        }

        private CachedMessage GetCachedMessage(ulong id) => Cache.TryGetValue(id, out CachedMessage message) ? message : null;

        public IMessage GetMessage(ulong id, bool includeRemoved = false)
        {
            var msg = GetCachedMessage(id);
            if (msg == null || (!includeRemoved && msg.Metadata.State == CachedMessageState.ToBeDeleted))
                return null;

            return msg.Message;
        }

        public async Task<IMessage> GetMessageAsync(IMessageChannel channel, ulong id)
        {
            var message = GetMessage(id);
            if (message != null) return message;

            message = await channel.GetMessageAsync(id);
            if (message != null)
                Cache.TryAdd(id, new CachedMessage(message));

            return message;
        }

        public async Task AppendAroundAsync(IMessageChannel channel, ulong id)
        {
            if (channel is IDMChannel) return;

            var messages = (await channel.GetMessagesAsync(id, Direction.Around).FlattenAsync())
                .Where(m => !Cache.ContainsKey(m.Id))
                .ToList();

            messages.ForEach(m => Cache.TryAdd(m.Id, new CachedMessage(m)));
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

        public async Task RunCheckAsync()
        {
            foreach (var (id, msg) in Cache.Where(o => o.Value.Metadata.State != CachedMessageState.None).ToDictionary(o => o.Key, o => o.Value))
            {
                if (msg.Metadata.State == CachedMessageState.ToBeDeleted)
                {
                    Cache.Remove(id, out var _);
                }
                else if (msg.Metadata.State == CachedMessageState.NeedsUpdate)
                {
                    var channel = msg.Message.Channel;
                    var newMessage = await channel.GetMessageAsync(id);

                    if (newMessage == null)
                    {
                        TryRemove(id, out var _);
                        continue;
                    }

                    Cache[id] = new CachedMessage(newMessage);
                }
            }
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

        public IEnumerable<IMessage> GetMessagesFromChannel(ulong channelId)
        {
            return Cache.Values
                .Where(o => !o.IsDeleted && o.Message.Channel.Id == channelId)
                .Select(o => o.Message);
        }

        public IEnumerable<IMessage> GetMessagesFromChannel(IChannel channel) => GetMessagesFromChannel(channel.Id);

        public IMessage GetLastMessageFromUserInChannel(IChannel channel, IUser user)
        {
            return GetMessagesFromChannel(channel)
                .Where(o => o.Author.Id == user.Id)
                .OrderByDescending(o => o.Id)
                .FirstOrDefault();
        }

        public IMessage GetLastCachedMessageFromUser(IUser user)
        {
            return Cache.Values
                .Where(o => !o.IsDeleted && o.Message.Author.Id == user.Id)
                .Select(o => o.Message)
                .FirstOrDefault();
        }
    }
}
