using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace GrillBot.App.Services
{
    public class AutoReplyService : ServiceBase
    {
        private string Prefix { get; }

        private List<ulong> DisabledChannels { get; }
        private ConcurrentBag<AutoReplyItem> Messages { get; }

        public AutoReplyService(IConfiguration configuration, DiscordSocketClient discordClient, GrillBotContextFactory dbFactory,
            DiscordInitializationService initializationService) : base(discordClient, dbFactory, initializationService)
        {
            DisabledChannels = configuration.GetSection("AutoReply:DisabledChannels").Get<ulong[]>()?.ToList() ?? new List<ulong>();
            Prefix = configuration["Discord:Commands:Prefix"];
            Messages = new ConcurrentBag<AutoReplyItem>();

            DiscordClient.Ready += InitAsync;
            DiscordClient.MessageReceived += (message) =>
            {
                if (!InitializationService.Get()) return Task.CompletedTask;
                if (!message.TryLoadMessage(out var userMessage)) return Task.CompletedTask;
                if (userMessage.IsCommand(DiscordClient.CurrentUser, Prefix)) return Task.CompletedTask;
                if (DisabledChannels.Contains(message.Channel.Id)) return Task.CompletedTask;

                return OnMessageReceivedAsync(userMessage);
            };
        }

        public async Task InitAsync()
        {
            using var dbContext = DbFactory.Create();
            var messages = await dbContext.AutoReplies
                .AsNoTracking().ToListAsync();

            Messages.Clear();
            foreach (var message in messages)
                Messages.Add(message);
        }

        private Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            var matched = Messages.Where(o => !o.IsDisabled)
                .FirstOrDefault(o => Regex.IsMatch(message.Content, o.Template, o.RegexOptions));

            if (matched == null) return Task.CompletedTask;
            return message.Channel.SendMessageAsync(matched.Reply);
        }
    }
}
