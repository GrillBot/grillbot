
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Handlers
{
    public class MessageHandler : Handler
    {
        private CommandService CommandService { get; }
        private IServiceProvider Provider { get; }
        private IConfiguration Configuration { get; }

        public MessageHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider provider, IConfiguration configuration) : base(client)
        {
            CommandService = commandService;
            Provider = provider;
            Configuration = configuration.GetSection("Discord:Commands");

            DiscordClient.MessageReceived += OnMessageReceived;
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (!TryLoadMessage(message, out SocketUserMessage userMessage)) return;
            var context = new SocketCommandContext(DiscordClient, userMessage);

            int argumentPosition = 0;
            if (IsCommand(userMessage, ref argumentPosition))
                await CommandService.ExecuteAsync(context, argumentPosition, Provider);
        }

        static private bool TryLoadMessage(SocketMessage message, out SocketUserMessage userMessage)
        {
            userMessage = null;

            if (message is not SocketUserMessage userMsg || !message.Author.IsUser())
                return false;

            userMessage = userMsg;
            return true;
        }

        private bool IsCommand(SocketUserMessage message, ref int argumentPosition)
        {
            if (message.HasMentionPrefix(DiscordClient.CurrentUser, ref argumentPosition))
                return true;

            var prefix = Configuration.GetValue<string>("Prefix");
            return message.Content.Length > prefix.Length && message.HasStringPrefix(prefix, ref argumentPosition);
        }
    }
}
