using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.MessageCache;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services.MessageCache
{
    public class MessageCache : ServiceBase
    {
        private ConcurrentDictionary<ulong, CachedMessage> Cache { get; }

        public MessageCache(DiscordSocketClient client) : base(client)
        {
            Cache = new ConcurrentDictionary<ulong, CachedMessage>();

            DiscordClient.Ready += OnReadyAsync;
            DiscordClient.MessageDeleted += (message, channel) =>
            {
                TryRemove(message.Id, out var _);
                return AppendAroundAsync(channel, message.Id);
            };
        }

        private async Task OnReadyAsync()
        {
            foreach (var chunk in DiscordClient.Guilds.SelectMany(o => o.TextChannels).SplitInParts(10))
            {
                foreach (var channel in chunk)
                {
                    var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                    messages.ForEach(o => Cache.TryAdd(o.Id, new CachedMessage(o)));
                }

                await Task.Delay(3000);
            }
        }

        private CachedMessage GetCachedMessage(ulong id) => Cache.TryGetValue(id, out CachedMessage message) ? message : null;

        public IMessage GetMessage(ulong id)
        {
            var msg = GetCachedMessage(id);
            if (msg == null || msg.Metadata.State == CachedMessageState.ToBeDeleted)
                return null;

            return msg.Message;
        }

        public async Task<IMessage> GetMessageAsync(ISocketMessageChannel channel, ulong id)
        {
            var message = GetMessage(id);
            if (message != null) return message;

            message = await channel.GetMessageAsync(id);
            if (message != null)
                Cache.TryAdd(id, new CachedMessage(message));

            return message;
        }

        public async Task AppendAroundAsync(ISocketMessageChannel channel, ulong id)
        {
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
            foreach (var (id, msg) in Cache.ToDictionary(o => o.Key, o => o.Value))
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
    }
}
