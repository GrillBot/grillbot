using Discord;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class MessageCache : ServiceBase
    {
        private ConcurrentDictionary<ulong, IMessage> Cache { get; }

        public MessageCache(DiscordSocketClient client) : base(client)
        {
            Cache = new ConcurrentDictionary<ulong, IMessage>();

            DiscordClient.Ready += OnReadyAsync;
            DiscordClient.MessageDeleted += (message, channel) => AppendAroundAsync(channel, message.Id);
        }

        private async Task OnReadyAsync()
        {
            foreach (var channel in DiscordClient.Guilds.SelectMany(o => o.TextChannels))
            {
                var messages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                messages.ForEach(o => Cache.TryAdd(o.Id, o));
            }
        }

        public IMessage GetMessage(ulong id)
        {
            return Cache.TryGetValue(id, out IMessage message) ? message : null;
        }

        public async Task<IMessage> GetMessageAsync(ISocketMessageChannel channel, ulong id)
        {
            var message = GetMessage(id);
            if (message != null) return message;

            message = await channel.GetMessageAsync(id);
            if (message != null)
                Cache.TryAdd(id, message);

            return message;
        }

        public async Task AppendAroundAsync(ISocketMessageChannel channel, ulong id)
        {
            var messages = (await channel.GetMessagesAsync(id, Direction.Around).FlattenAsync())
                .Where(m => !Cache.ContainsKey(m.Id))
                .ToList();

            messages.ForEach(m => Cache.TryAdd(m.Id, m));
        }

        public bool TryRemove(ulong id, out IMessage message) => Cache.TryRemove(id, out message);
    }
}
