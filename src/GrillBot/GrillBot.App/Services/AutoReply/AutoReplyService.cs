using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System.Text.RegularExpressions;

namespace GrillBot.App.Services.AutoReply;

[Initializable]
public class AutoReplyService : ServiceBase
{
    private string Prefix { get; }

    private List<ulong> DisabledChannels { get; }
    private List<AutoReplyItem> Messages { get; }
    private SemaphoreSlim Semaphore { get; }

    private InitManager InitManager { get; }

    public AutoReplyService(IConfiguration configuration, DiscordSocketClient discordClient, GrillBotContextFactory dbFactory,
        InitManager initManager) : base(discordClient, dbFactory)
    {
        Prefix = configuration["Discord:Commands:Prefix"];
        Messages = new List<AutoReplyItem>();
        DisabledChannels = new List<ulong>();
        Semaphore = new(1);
        InitManager = initManager;

        DiscordClient.Ready += () => InitAsync();
        DiscordClient.MessageReceived += (message) =>
        {
            if (!InitManager.Get()) return Task.CompletedTask;
            if (!message.TryLoadMessage(out var userMessage)) return Task.CompletedTask;
            if (userMessage.IsCommand(DiscordClient.CurrentUser, Prefix)) return Task.CompletedTask;
            if (DisabledChannels.Contains(message.Channel.Id)) return Task.CompletedTask;

            return OnMessageReceivedAsync(userMessage);
        };
    }

    public async Task InitAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            using var dbContext = DbFactory.Create();
            var messages = await dbContext.AutoReplies.AsNoTracking().ToListAsync();

            Messages.Clear();
            Messages.AddRange(messages);

            var disabledChannels = await dbContext.Channels.AsNoTracking()
                .Where(o => (o.Flags & (long)ChannelFlags.Deleted) == 0 && (o.Flags & (long)ChannelFlags.AutoReplyDeactivated) != 0)
                .Select(o => o.ChannelId)
                .ToListAsync();

            DisabledChannels.Clear();
            DisabledChannels.AddRange(
                disabledChannels.Select(o => o.ToUlong())
            );
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnMessageReceivedAsync(SocketUserMessage message)
    {
        await Semaphore.WaitAsync();

        try
        {
            var matched = Messages
                .Find(o => !o.IsDisabled && Regex.IsMatch(message.Content, o.Template, o.RegexOptions));

            if (matched == null) return;
            await message.Channel.SendMessageAsync(matched.Reply);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
