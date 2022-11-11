using System.Text.RegularExpressions;
using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services;

[Initializable]
public class AutoReplyService
{
    private List<ulong> DisabledChannels { get; }
    private List<AutoReplyItem> Messages { get; }
    private SemaphoreSlim Semaphore { get; } = new(1);

    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AutoReplyService(DiscordSocketClient discordClient, GrillBotDatabaseBuilder databaseBuilder, InitManager initManager)
    {
        Messages = new List<AutoReplyItem>();
        DisabledChannels = new List<ulong>();
        InitManager = initManager;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;

        DiscordClient.Ready += InitAsync;
        DiscordClient.MessageReceived += OnMessageReceivedAsync;
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

            var supportedChannelTypes = new List<ChannelType> { ChannelType.Forum, ChannelType.Stage, ChannelType.Text, ChannelType.Voice, ChannelType.PrivateThread, ChannelType.PublicThread };
            var disabledChannels = await repository.Channel.GetAllChannelsAsync(true, false, false, supportedChannelTypes);
            disabledChannels = disabledChannels.FindAll(o => o.HasFlag(ChannelFlags.AutoReplyDeactivated));

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

    private async Task OnMessageReceivedAsync(IMessage message)
    {
        if (!CanReact(message)) return;
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

    private bool CanReact(IMessage message)
    {
        return InitManager.Get() && message.TryLoadMessage(out var userMessage) && !userMessage!.IsInteractionCommand() &&
               !DisabledChannels.Contains(message.Channel.Id);
    }
}
