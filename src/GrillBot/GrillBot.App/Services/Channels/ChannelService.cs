using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;

namespace GrillBot.App.Services.Channels;

[Initializable]
public class ChannelService
{
    private string CommandPrefix { get; }
    private MessageCacheManager MessageCache { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration,
        MessageCacheManager messageCache)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        MessageCache = messageCache;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageReceived += message => message.TryLoadMessage(out var msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketUserMessage message)
    {
        var argPos = 0;

        // Commands and DM in channelboard is not allowed.
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return;
        if (message.Channel is not ITextChannel textChannel) return;
        if (message.Author is not IGuildUser author) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.GetOrCreateChannelAsync(textChannel, true);
        var user = await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        var userChannel = channel.Users.FirstOrDefault(o => o.UserId == user.UserId);

        if (userChannel == null)
        {
            userChannel = new Database.Entity.GuildUserChannel
            {
                User = user,
                FirstMessageAt = DateTime.Now,
                Count = 0
            };

            channel.Users.Add(userChannel);
        }

        userChannel.Count++;
        userChannel.LastMessageAt = DateTime.Now;
        await repository.CommitAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (!messageChannel.HasValue || msg == null || messageChannel.Value is not ITextChannel channel) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true);
        var userChannel = channelEntity?.Users.FirstOrDefault(o => o.UserId == msg.Author.Id.ToString());
        if (userChannel == null) return;

        userChannel.Count--;
        await repository.CommitAsync();
    }

    private async Task<List<SocketTextChannel>> GetTopMostActiveChannelsOfUserAsync(IUser user, IGuild guild, int take)
    {
        await using var repositor = DatabaseBuilder.CreateRepository();

        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        var topChannels = await repositor.Channel.GetTopChannelsOfUserAsync(guildUser, take, true);

        // User not have any active channel.
        if (topChannels.Count == 0) return new List<SocketTextChannel>();

        var channelIds = topChannels.ConvertAll(o => o.ChannelId);
        var channels = new List<SocketTextChannel>();
        foreach (var channelId in channelIds)
        {
            if (await guild.GetTextChannelAsync(channelId.ToUlong()) is SocketTextChannel channel)
                channels.Add(channel);
        }

        return channels;
    }

    /// <summary>
    /// Finds last message from user in cache. If message wasn't found bot will use statistics and refresh cache and tries find message.
    /// </summary>
    public async Task<IUserMessage> GetLastMsgFromUserAsync(SocketGuild guild, IUser loggedUser)
    {
        var lastCachedMsgFromAuthor = await MessageCache.GetLastMessageAsync(guild: guild, author: loggedUser);
        if (lastCachedMsgFromAuthor is IUserMessage lastMessage) return lastMessage;

        // Using statistics and finding most active channel will help find channel where logged user have any message.
        // This eliminates the need to browser channels and finds some activity.
        var mostActiveChannels = await GetTopMostActiveChannelsOfUserAsync(loggedUser, guild, 10);
        foreach (var channel in mostActiveChannels)
        {
            lastMessage = await TryFindLastMessageFromUserAsync(channel, loggedUser, true);
            if (lastMessage != null) return lastMessage;
        }

        return guild.TextChannels
            .SelectMany(o => o.CachedMessages)
            .Where(o => o.Author.Id == loggedUser.Id)
            .MaxBy(o => o.Id) as IUserMessage;
    }

    private async Task<IUserMessage> TryFindLastMessageFromUserAsync(ISocketMessageChannel channel, IUser loggedUser, bool canTryDownload)
    {
        var lastMessage = new[]
        {
            channel.CachedMessages.Where(o => o.Author.Id == loggedUser.Id).MaxBy(o => o.Id),
            await MessageCache.GetLastMessageAsync(channel: channel, author: loggedUser)
        }.Where(o => o != null).MaxBy(o => o.Id);

        if (lastMessage != null)
            return (IUserMessage)lastMessage;

        if (!canTryDownload)
            return null;

        // Try reload cache and try find message.
        await MessageCache.DownloadMessagesAsync(channel);
        return await TryFindLastMessageFromUserAsync(channel, loggedUser, false);
    }
}
