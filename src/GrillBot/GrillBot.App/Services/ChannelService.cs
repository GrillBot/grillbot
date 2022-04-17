using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;

namespace GrillBot.App.Services;

[Initializable]
public class ChannelService : ServiceBase
{
    private string CommandPrefix { get; }
    private MessageCache.MessageCache MessageCache { get; }

    public ChannelService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
        MessageCache.MessageCache messageCache, IMapper mapper) : base(client, dbFactory, null, null, mapper)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        MessageCache = messageCache;

        DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
        DiscordClient.ChannelDestroyed += (channel) => channel is SocketTextChannel chnl ? OnGuildChannelRemovedAsync(chnl) : Task.CompletedTask;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketUserMessage message)
    {
        int argPos = 0;

        // Commands and DM in channelboard is not allowed.
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return;
        if (message.Channel is not SocketTextChannel textChannel) return;

        using var dbContext = DbFactory.Create();

        var guildId = textChannel.Guild.Id.ToString();
        var channelId = textChannel.Id.ToString();
        var userId = message.Author.Id.ToString();

        // Check DB for consistency.
        await dbContext.InitGuildAsync(textChannel.Guild, CancellationToken.None);
        await dbContext.InitUserAsync(message.Author, CancellationToken.None);
        await dbContext.InitGuildUserAsync(textChannel.Guild, message.Author as IGuildUser, CancellationToken.None);
        var channelType = DiscordHelper.GetChannelType(textChannel);
        await dbContext.InitGuildChannelAsync(textChannel.Guild, textChannel, channelType.Value, CancellationToken.None);

        // Search specific channel for specific guild and user.
        var channel = await dbContext.UserChannels.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.ChannelId == channelId && o.UserId == userId);

        if (channel == null)
        {
            channel = new Database.Entity.GuildUserChannel()
            {
                UserId = userId,
                GuildId = guildId,
                ChannelId = channelId,
                FirstMessageAt = DateTime.Now,
                Count = 0
            };

            await dbContext.AddAsync(channel);
        }

        channel.Count++;
        channel.LastMessageAt = DateTime.Now;
        await dbContext.SaveChangesAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        var msg = message.HasValue ? message.Value : MessageCache.GetMessage(message.Id);
        if (!messageChannel.HasValue || msg == null || messageChannel.Value is not SocketTextChannel channel) return;

        var guildId = channel.Guild.Id.ToString();
        var userId = msg.Author.Id.ToString();
        var channelId = channel.Id.ToString();

        using var dbContext = DbFactory.Create();

        var dbChannel = await dbContext.UserChannels.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId && o.ChannelId == channelId);

        if (dbChannel == null) return;

        dbChannel.Count--;
        await dbContext.SaveChangesAsync();
    }

    private async Task OnGuildChannelRemovedAsync(SocketTextChannel channel)
    {
        var guildId = channel.Guild.Id.ToString();
        var channelId = channel.Id.ToString();

        using var dbContext = DbFactory.Create();

        var channelsQuery = dbContext.UserChannels.AsQueryable().Where(o => o.ChannelId == channelId && o.GuildId == guildId);
        var channels = await channelsQuery.ToListAsync();

        dbContext.RemoveRange(channels);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<SocketTextChannel>> GetTopMostActiveChannelsOfUserAsync(IUser user, IGuild guild, int take, CancellationToken cancellationToken)
    {
        using var dbContext = DbFactory.Create();

        var channelIdQuery = dbContext.UserChannels.AsNoTracking()
            .Where(o => o.Channel.ChannelType == ChannelType.Text && o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString() && o.Count > 0)
            .OrderByDescending(o => o.Count)
            .Select(o => o.ChannelId)
            .Take(take);

        var channelIds = await channelIdQuery.ToListAsync(cancellationToken);

        // User not have any active channel.
        if (channelIds.Count == 0) return new();

        var channels = new List<SocketTextChannel>();
        foreach (var channelId in channelIds)
        {
            if ((await guild.GetTextChannelAsync(Convert.ToUInt64(channelId))) is SocketTextChannel channel) channels.Add(channel);
        }

        return channels;
    }

    /// <summary>
    /// Finds last message from user in cache. If message wasn't found bot will use statistics and refresh cache and tries find message.
    /// </summary>
    public async Task<IUserMessage> GetLastMsgFromUserAsync(SocketGuild guild, IUser loggedUser, CancellationToken cancellationToken)
    {
        var lastCachedMsgFromAuthor = await MessageCache.GetLastMessageAsync(guild: guild, author: loggedUser, cancellationToken: cancellationToken);
        if (lastCachedMsgFromAuthor is IUserMessage lastMessage) return lastMessage;

        // Using statistics and finding most active channel will help find channel where logged user have any message.
        // This eliminates the need to browser channels and finds some activity.
        var mostActiveChannels = await GetTopMostActiveChannelsOfUserAsync(loggedUser, guild, 10, cancellationToken);
        foreach (var channel in mostActiveChannels)
        {
            lastMessage = await TryFindLastMessageFromUserAsync(channel, loggedUser, true, cancellationToken);
            if (lastMessage != null) return lastMessage;
        }

        return guild.TextChannels
            .SelectMany(o => o.CachedMessages)
            .Where(o => o.Author.Id == loggedUser.Id)
            .OrderByDescending(o => o.Id)
            .FirstOrDefault() as IUserMessage;
    }

    private async Task<IUserMessage> TryFindLastMessageFromUserAsync(SocketTextChannel channel, IUser loggedUser, bool canTryDownload, CancellationToken cancellationToken = default)
    {
        var lastMessage = new[]
        {
                channel.CachedMessages.Where(o => o.Author.Id == loggedUser.Id).OrderByDescending(o => o.Id).FirstOrDefault(),
                await MessageCache.GetLastMessageAsync(channel: channel, author: loggedUser, cancellationToken: cancellationToken)
            }.Where(o => o != null).OrderByDescending(o => o.Id).FirstOrDefault();

        if (lastMessage == null && canTryDownload)
        {
            // Try reload cache and try find message.
            await MessageCache.DownloadLatestFromChannelAsync(channel, cancellationToken);
            return await TryFindLastMessageFromUserAsync(channel, loggedUser, false, cancellationToken);
        }

        return lastMessage as IUserMessage;
    }

    public async Task<PaginatedResponse<GuildChannelListItem>> GetChannelListAsync(GetChannelListParams parameters, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.Channels.AsNoTracking()
            .Include(o => o.Guild)
            .Include(o => o.Users.Where(o => o.Count > 0))
            .ThenInclude(o => o.User.User)
            .AsQueryable();

        query = parameters.CreateQuery(query);
        return await PaginatedResponse<GuildChannelListItem>
            .CreateAsync(query, parameters, (entity, cancellationToken) => ConvertChannelDbEntityAsync(entity, cancellationToken), cancellationToken);
    }

    private async Task<GuildChannelListItem> ConvertChannelDbEntityAsync(Database.Entity.GuildChannel entity, CancellationToken cancellationToken)
    {
        var guildChannel = GetChannelFromEntity(entity);

        var result = Mapper.Map<GuildChannelListItem>(entity);

        if (guildChannel != null)
            result = Mapper.Map(guildChannel, result);

        result.CachedMessagesCount = await MessageCache.GetMessagesCountAsync(channelId: Convert.ToUInt64(entity.ChannelId), cancellationToken: cancellationToken);
        if (result.FirstMessageAt == DateTime.MinValue) result.FirstMessageAt = null;
        if (result.LastMessageAt == DateTime.MinValue) result.LastMessageAt = null;

        return result;
    }

    public async Task<ChannelDetail> GetChannelDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.Channels.AsNoTracking()
            .Include(o => o.Guild)
            .Include(o => o.Users.Where(o => o.Count > 0)).ThenInclude(o => o.User.User)
            .Include(o => o.ParentChannel);

        var channel = await query.FirstOrDefaultAsync(o => o.ChannelId == id.ToString(), cancellationToken);
        if (channel == null) return null;

        var result = Mapper.Map<ChannelDetail>(channel);
        result.CachedMessagesCount = await MessageCache.GetMessagesCountAsync(channelId: Convert.ToUInt64(channel.ChannelId), cancellationToken: cancellationToken);

        var guildChannel = GetChannelFromEntity(channel);
        if (guildChannel != null)
            result = Mapper.Map(guildChannel, result);

        return result;
    }

    private SocketGuildChannel GetChannelFromEntity(Database.Entity.GuildChannel entity)
    {
        var guild = DiscordClient.GetGuild(Convert.ToUInt64(entity.GuildId));
        return guild?.GetChannel(Convert.ToUInt64(entity.ChannelId));
    }
}
