using System.Diagnostics.CodeAnalysis;
using GrillBot.Common.Managers;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Managers;

public class PinManager
{
    private SemaphoreSlim Lock { get; }
    private HashSet<ulong> InitializedChannels { get; }
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private ICounterManager CounterManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PinManager(InitManager initManager, IDiscordClient discordClient, ICounterManager counterManager, GrillBotDatabaseBuilder databaseBuilder)
    {
        Lock = new SemaphoreSlim(1);
        InitializedChannels = new HashSet<ulong>();
        DiscordClient = (DiscordSocketClient)discordClient;
        InitManager = initManager;
        CounterManager = counterManager;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageReceived += async msg => await OnEventAsync(msg.Channel);
        DiscordClient.ChannelUpdated += async (_, after) => await OnEventAsync(after);
        DiscordClient.InteractionCreated += async cmd => await OnEventAsync(cmd.Channel);
        DiscordClient.MessageDeleted += async (_, channel) => await OnEventAsync(channel.HasValue ? channel.Value : null);
        DiscordClient.MessageUpdated += async (_, _, channel) => await OnEventAsync(channel);
        DiscordClient.ReactionAdded += async (_, channel, _) => await OnEventAsync(channel.HasValue ? channel.Value : null);
        DiscordClient.ReactionRemoved += async (_, channel, _) => await OnEventAsync(channel.HasValue ? channel.Value : null);
        DiscordClient.ThreadUpdated += async (_, after) => await OnEventAsync(after);
    }

    private static bool IsValidChannel(IChannel? channel, [MaybeNullWhen(false)] out ITextChannel textChannel)
    {
        textChannel = channel as ITextChannel;
        return textChannel is not null && channel is not IVoiceChannel;
    }

    #region Events

    private async Task OnEventAsync(IChannel? channel)
    {
        if (!IsValidChannel(channel, out var textChannel) || !InitManager.Get())
            return;

        await Lock.WaitAsync();
        try
        {
            if (InitializedChannels.Contains(textChannel.Id))
                return;
        }
        finally
        {
            Lock.Release();
        }

        await UpdatePinCountAsync(textChannel);
    }

    #endregion

    private async Task UpdatePinCountAsync(ITextChannel channel)
    {
        int pinCount;
        using (CounterManager.Create("Discord.API.Messages"))
            pinCount = (await channel.GetPinnedMessagesAsync()).Count;

        await using var repository = DatabaseBuilder.CreateRepository();

        var dbChannel = await repository.Channel.GetOrCreateChannelAsync(channel);
        dbChannel.PinCount = pinCount;

        await repository.CommitAsync();
        InitializedChannels.Add(channel.Id);
    }
}
