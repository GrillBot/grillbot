using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Services
{
    public class EmoteChainService : ServiceBase
    {
        // Dictionary<GuildID|ChannelID, List<UserID, Message>>
        private Dictionary<string, List<Tuple<ulong, string>>> LastMessages { get; }
        private int RequiredCount { get; }

        private readonly object Locker = new();

        public EmoteChainService(IConfiguration configuration, DiscordSocketClient client) : base(client)
        {
            RequiredCount = configuration.GetValue<int>("Emotes:ChainRequiredCount");
            LastMessages = new Dictionary<string, List<Tuple<ulong, string>>>();

            DiscordClient.MessageReceived += (msg) => msg.TryLoadMessage(out var message) ? OnMessageReceivedAsync(message) : Task.CompletedTask;
        }

        public void Cleanup(IGuildChannel channel)
        {
            lock (Locker)
            {
                CleanupNoLock(channel);
            }
        }

        public void CleanupNoLock(IGuildChannel channel)
        {
            var key = GetKey(channel);
            if (LastMessages.ContainsKey(key)) LastMessages[key].Clear();
        }

        private Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            if (RequiredCount < 1) return Task.CompletedTask;
            var context = new CommandContext(DiscordClient, message);
            if (context.IsPrivate) return Task.CompletedTask;

            return ProcessChainAsync(context);
        }

        private async Task ProcessChainAsync(ICommandContext context)
        {
            if (context.Channel is not SocketTextChannel channel) return;
            var author = context.Message.Author;
            var content = context.Message.Content;
            var key = GetKey(channel);

            if (!LastMessages.ContainsKey(key))
                LastMessages.Add(key, new List<Tuple<ulong, string>>(RequiredCount));

            if (!IsValidMessage(context.Message, context.Guild, channel))
            {
                CleanupNoLock(channel);
                return;
            }

            var group = LastMessages[key];

            if (!group.Any(o => o.Item1 == author.Id))
                group.Add(new Tuple<ulong, string>(author.Id, content));

            if (group.Count == RequiredCount)
            {
                await channel.SendMessageAsync(group[0].Item2);
                CleanupNoLock(channel);
            }
        }

        private bool IsValidWithWithFirstInChannel(IGuildChannel channel, string content)
        {
            var key = GetKey(channel);
            var group = LastMessages[key];

            if (group.Count == 0)
                return true;

            return content == group[0].Item2;
        }

        private bool IsValidMessage(IUserMessage message, IGuild guild, IGuildChannel channel)
        {
            var emotes = message.Tags
               .Where(o => o.Type == TagType.Emoji && guild.Emotes.Any(x => x.Id == o.Key))
               .ToList();

            var isUTFEmoji = NeoSmart.Unicode.Emoji.IsEmoji(message.Content);
            if (emotes.Count == 0 && !isUTFEmoji) return false;

            if (!IsValidWithWithFirstInChannel(channel, message.Content)) return false;
            var emoteTemplate = string.Join(" ", emotes.Select(o => o.Value.ToString()));
            return emoteTemplate == message.Content || isUTFEmoji;
        }

        private static string GetKey(IGuildChannel channel) => $"{channel.Guild.Id}|{channel.Id}";
    }
}
