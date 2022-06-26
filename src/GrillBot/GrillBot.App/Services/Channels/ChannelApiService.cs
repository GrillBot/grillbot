using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.AutoReply;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using System.Security.Claims;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Models;
using GrillBot.Database.Enums.Internal;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Channels;

public class ChannelApiService
{
    private MessageCacheManager MessageCache { get; }
    private AutoReplyService AutoReplyService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public ChannelApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient client, MessageCacheManager messageCache,
        AutoReplyService autoReplyService, ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        MessageCache = messageCache;
        AutoReplyService = autoReplyService;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = client;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<PaginatedResponse<GuildChannelListItem>> GetListAsync(GetChannelListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Channel.GetChannelListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<GuildChannelListItem>.CopyAndMapAsync(data, MapChannelAsync);
    }

    private async Task<GuildChannelListItem> MapChannelAsync(Database.Entity.GuildChannel entity)
    {
        var guild = await DiscordClient.GetGuildAsync(entity.GuildId.ToUlong());
        var guildChannel = guild != null ? await guild.GetChannelAsync(entity.ChannelId.ToUlong()) : null;

        var result = Mapper.Map<GuildChannelListItem>(entity);
        if (guildChannel != null)
        {
            result = Mapper.Map(guildChannel, result);

            if (entity.IsText() || entity.IsThread() || entity.IsVoice())
                result.CachedMessagesCount = await MessageCache.GetCachedMessagesCount(guildChannel);
        }

        if (result.FirstMessageAt == DateTime.MinValue) result.FirstMessageAt = null;
        if (result.LastMessageAt == DateTime.MinValue) result.LastMessageAt = null;

        return result;
    }

    public async Task<ChannelDetail> GetDetailAsync(ulong id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id, null, true, ChannelsIncludeUsersMode.IncludeExceptInactive, true, true);
        if (channel == null) return null;

        var result = Mapper.Map<ChannelDetail>(channel);
        if (channel.IsText())
        {
            var threads = await repository.Channel.GetChildChannelsAsync(id);
            result.Threads = Mapper.Map<List<Channel>>(threads);
        }

        if (channel.HasFlag(ChannelFlags.Deleted))
            return result;

        var guild = await DiscordClient.GetGuildAsync(channel.GuildId.ToUlong());
        var guildChannel = guild != null ? await guild.GetChannelAsync(channel.ChannelId.ToUlong()) : null;
        if (guildChannel == null)
            return result;

        result = Mapper.Map(guildChannel, result);
        if (channel.IsText() || channel.IsThread() || channel.IsVoice())
            result.CachedMessagesCount = await MessageCache.GetCachedMessagesCount(guildChannel);
        return result;
    }

    public async Task<bool> UpdateChannelAsync(ulong id, UpdateChannelParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id);
        if (channel == null)
            throw new NotFoundException();

        var reloadAutoReply = channel.HasFlag(ChannelFlags.AutoReplyDeactivated) != ((parameters.Flags & (long)ChannelFlags.AutoReplyDeactivated) != 0);

        channel.Flags = parameters.Flags;
        var success = await repository.CommitAsync() > 0;

        if (reloadAutoReply && success)
            await AutoReplyService.InitAsync();

        return success;
    }

    public async Task PostMessageAsync(ulong guildId, ulong channelId, SendMessageToChannelParams parameters)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        if (guild == null)
            throw new NotFoundException("Nepodařilo se najít server.");

        var channel = await guild.GetTextChannelAsync(channelId);
        if (channel == null)
            throw new NotFoundException($"Nepodařilo se najít kanál na serveru {guild.Name}");

        var reference = MessageHelper.CreateMessageReference(parameters.Reference, channelId, guildId);
        await channel.SendMessageAsync(parameters.Content, messageReference: reference);
    }

    public async Task ClearCacheAsync(ulong guildId, ulong channelId, ClaimsPrincipal user)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        if (guild == null) return;

        var channel = await guild.GetTextChannelAsync(channelId);
        if (channel == null)
            return;

        var clearedCount = MessageCache.ClearAllMessagesFromChannel(channel);

        var auditLogItem = new AuditLogDataWrapper(
            AuditLogItemType.Info,
            $"Byla ručně smazána cache zpráv kanálu. Smazaných zpráv: {clearedCount}",
            guild, channel, ApiRequestContext.LoggedUser
        );

        await AuditLogWriter.StoreAsync(auditLogItem);
    }

    public async Task<PaginatedResponse<ChannelUserStatItem>> GetChannelUsersAsync(ulong channelId, PaginatedParams pagination)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Channel.GetUserChannelListAsync(channelId, pagination);
        var result = await PaginatedResponse<ChannelUserStatItem>
            .CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<ChannelUserStatItem>(entity)));

        for (var i = 0; i < result.Data.Count; i++)
            result.Data[i].Position = pagination.Skip + i + 1;

        return result;
    }

    public async Task<List<ChannelboardItem>> GetChannelBoardAsync()
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);
        var result = new List<ChannelboardItem>();

        foreach (var guild in mutualGuilds)
            result.AddRange(await GetChannelBoardOfGuildAsync(loggedUserId, guild));

        return result
            .OrderByDescending(o => o.Count)
            .ThenByDescending(o => o.LastMessageAt)
            .ToList();
    }

    private async Task<List<ChannelboardItem>> GetChannelBoardOfGuildAsync(ulong loggedUserId, IGuild guild)
    {
        var guildUser = await guild.GetUserAsync(loggedUserId);

        var availableChannels = await guild.GetAvailableChannelsAsync(guildUser, true);
        if (availableChannels.Count == 0) return new List<ChannelboardItem>();
        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelStats = await repository.Channel.GetAvailableStatsAsync(guild, availableChannelIds);
        if (channelStats.Count == 0) return new List<ChannelboardItem>();

        var channelStatIds = channelStats.ConvertAll(o => o.channelId);
        var channels = await repository.Channel.GetVisibleChannelsAsync(guild.Id, channelStatIds, true, true);

        var result = new List<ChannelboardItem>();
        foreach (var channel in channels)
        {
            var stats = channelStats.Find(o => o.channelId == channel.ChannelId);

            var channelboardItem = Mapper.Map<ChannelboardItem>(channel);
            channelboardItem.Count = stats.count;
            channelboardItem.LastMessageAt = stats.lastMessageAt;
            channelboardItem.FirstMessageAt = stats.firstMessageAt;

            result.Add(channelboardItem);
        }

        return result;
    }
}
