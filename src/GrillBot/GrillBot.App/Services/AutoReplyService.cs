using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class AutoReplyService : ServiceBase
    {
        private string Prefix { get; }

        private List<ulong> DisabledChannels { get; }
        private List<AutoReplyConfiguration> Messages { get; }

        public AutoReplyService(IConfiguration configuration, DiscordSocketClient discordClient) : base(discordClient)
        {
            var config = configuration.GetSection("AutoReply");
            DisabledChannels = config.GetSection("DisabledChannels").Get<ulong[]>()?.ToList() ?? new List<ulong>();
            Messages = config.GetSection("Messages").Get<AutoReplyConfiguration[]>().ToList();

            Prefix = configuration["Discord:Commands:Prefix"];

            DiscordClient.MessageReceived += (message) =>
            {
                // Block commands, system messages and bots.
                if (message is not SocketUserMessage msg || !message.Author.IsUser()) return Task.CompletedTask;

                int argPos = 0;
                var canProcess = !msg.HasMentionPrefix(DiscordClient.CurrentUser, ref argPos) && !msg.HasStringPrefix(Prefix, ref argPos);

                return canProcess ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
            };
        }

        private Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            if (DisabledChannels.Contains(message.Channel.Id)) return Task.CompletedTask;

            var matched = Messages.Where(o => !o.Disabled)
                .FirstOrDefault(o => Regex.IsMatch(message.Content, o.Template, o.Options));

            if (matched == null) return Task.CompletedTask;
            return message.Channel.SendMessageAsync(matched.Reply);
        }
    }
}
