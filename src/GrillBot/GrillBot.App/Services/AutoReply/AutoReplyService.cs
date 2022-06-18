using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System.Text.RegularExpressions;

namespace GrillBot.App.Services.AutoReply;

[Initializable]
public class AutoReplyService
{
    private string Prefix { get; }

    private List<ulong> DisabledChannels { get; }
    private List<AutoReplyItem> Messages { get; }
    private SemaphoreSlim Semaphore { get; } = new(1);

    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AutoReplyService(IConfiguration configuration, DiscordSocketClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        InitManager initManager)
    {
        Prefix = configuration["Discord:Commands:Prefix"];
        Messages = new List<AutoReplyItem>();
        DisabledChannels = new List<ulong>();
        InitManager = initManager;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;

        DiscordClient.Ready += InitAsync;
        DiscordClient.MessageReceived += message =>
        {
            if (!InitManager.Get()) return Task.CompletedTask;
            if (!message.TryLoadMessage(out var userMessage)) return Task.CompletedTask;
            if (userMessage.IsCommand(DiscordClient.CurrentUser, Prefix)) return Task.CompletedTask;
            return DisabledChannels.Contains(message.Channel.Id) ? Task.CompletedTask : OnMessageReceivedAsync(userMessage);
        };
    }

    public async Task InitAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            await using var repository = DatabaseBuilder.CreateRepository();
            var messages = await repository.AutoReply.GetAllAsync();

            Messages.Clear();
            Messages.AddRange(messages);

            var disabledChannels = await repository.Channel.GetAllChannelsAsync(true, false);
            disabledChannels = disabledChannels.FindAll(o => (o.Flags & (long)ChannelFlags.AutoReplyDeactivated) != 0);

            DisabledChannels.Clear();
            DisabledChannels.AddRange(
                disabledChannels.Select(o => o.ChannelId.ToUlong())
            );
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        await Semaphore.WaitAsync();

        try
        {
            var matched = Messages
                .Find(o => !o.HaveFlags(AutoReplyFlags.Disabled) && Regex.IsMatch(message.Content, o.Template, o.RegexOptions));

            if (matched == null) return;
            await message.Channel.SendMessageAsync(matched.Reply);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
