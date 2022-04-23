using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Helper;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using System.Security.Claims;

namespace GrillBot.App.Services.Channels;

public class ChannelApiService : ServiceBase
{
    private MessageCache.MessageCache MessageCache { get; }
    private AuditLogService AuditLogService { get; }

    public ChannelApiService(GrillBotContextFactory dbFactory, IMapper mapper, IDiscordClient client, MessageCache.MessageCache messageCache,
        AuditLogService auditLogService) : base(null, dbFactory, null, client, mapper)
    {
        MessageCache = messageCache;
        AuditLogService = auditLogService;
    }

    public async Task<PaginatedResponse<GuildChannelListItem>> GetListAsync(GetChannelListParams parameters, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);

        return await PaginatedResponse<GuildChannelListItem>
            .CreateAsync(query, parameters.Pagination, (entity, cancellationToken) => MapChannelAsync(entity, cancellationToken), cancellationToken);
    }

    private async Task<GuildChannelListItem> MapChannelAsync(Database.Entity.GuildChannel entity, CancellationToken cancellationToken = default)
    {
        var guild = await DcClient.GetGuildAsync(Convert.ToUInt64(entity.GuildId));
        var guildChannel = guild != null ? await guild.GetChannelAsync(Convert.ToUInt64(entity.ChannelId)) : null;

        var result = Mapper.Map<GuildChannelListItem>(entity);
        if (guildChannel != null)
        {
            result = Mapper.Map(guildChannel, result);
            result.CachedMessagesCount = await MessageCache.GetMessagesCountAsync(channelId: Convert.ToUInt64(entity.ChannelId), cancellationToken: cancellationToken);
        }

        if (result.FirstMessageAt == DateTime.MinValue) result.FirstMessageAt = null;
        if (result.LastMessageAt == DateTime.MinValue) result.LastMessageAt = null;

        return result;
    }

    public async Task<ChannelDetail> GetDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.Channels.AsNoTracking()
            .Include(o => o.Guild)
            .Include(o => o.Users.Where(o => o.Count > 0)).ThenInclude(o => o.User.User)
            .Include(o => o.ParentChannel);

        var channel = await query.FirstOrDefaultAsync(o => o.ChannelId == id.ToString(), cancellationToken);
        if (channel == null) return null;

        var result = Mapper.Map<ChannelDetail>(channel);

        var guild = await DcClient.GetGuildAsync(Convert.ToUInt64(channel.GuildId));
        var guildChannel = guild != null ? await guild.GetChannelAsync(Convert.ToUInt64(channel.ChannelId)) : null;
        if (guildChannel != null)
        {
            result = Mapper.Map(guildChannel, result);
            result.CachedMessagesCount = await MessageCache.GetMessagesCountAsync(channelId: Convert.ToUInt64(channel.ChannelId), cancellationToken: cancellationToken);
        }

        return result;
    }

    public async Task<bool> UpdateChannelAsync(ulong id, UpdateChannelParams parameters)
    {
        using var context = DbFactory.Create();

        var channel = await context.Channels
            .FirstOrDefaultAsync(o => o.ChannelId == id.ToString());

        if (channel == null)
            throw new NotFoundException();

        channel.Flags = parameters.Flags;
        return (await context.SaveChangesAsync()) > 0;
    }

    public async Task PostMessageAsync(ulong guildId, ulong channelId, SendMessageToChannelParams parameters)
    {
        var guild = await DcClient.GetGuildAsync(guildId);
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
        var clearedCount = MessageCache.ClearChannel(channelId);

        var guild = await DcClient.GetGuildAsync(guildId);
        var auditLogItem = new AuditLogDataWrapper(
            AuditLogItemType.Info,
            $"Byla manuálně smazána cache zpráv kanálu. Smazaných zpráv: {clearedCount}",
            guild,
            guild == null ? null : await guild.GetTextChannelAsync(channelId),
            await DcClient.FindUserAsync(user.GetUserId())
        );

        await AuditLogService.StoreItemAsync(auditLogItem);
    }

    public async Task<PaginatedResponse<ChannelUserStatItem>> GetChannelUsersAsync(ulong channelId, PaginatedParams pagination, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.UserChannels.AsNoTracking()
            .Include(o => o.User.User)
            .OrderByDescending(o => o.Count)
            .Where(o => o.ChannelId == channelId.ToString() && o.Count > 0);

        var result = await PaginatedResponse<ChannelUserStatItem>.CreateAsync(query, pagination,
            entity => Mapper.Map<ChannelUserStatItem>(entity), cancellationToken);

        for (int i = 0; i < result.Data.Count; i++)
            result.Data[i].Position = pagination.Skip + i + 1;

        return result;
    }

    public async Task<List<ChannelboardItem>> GetChannelBoardAsync(ClaimsPrincipal loggedUser, CancellationToken cancellationToken = default)
    {
        var loggedUserId = loggedUser.GetUserId();
        var mutualGuilds = await DcClient.FindMutualGuildsAsync(loggedUserId);
        var result = new List<ChannelboardItem>();

        foreach (var guild in mutualGuilds)
            result.AddRange(await GetChannelBoardOfGuildAsync(loggedUserId, guild, cancellationToken));

        return result
            .OrderByDescending(o => o.Count)
            .ThenByDescending(o => o.LastMessageAt)
            .ToList();
    }

    private async Task<List<ChannelboardItem>> GetChannelBoardOfGuildAsync(ulong loggedUserId, IGuild guild, CancellationToken cancellationToken = default)
    {
        var guildUser = await guild.GetUserAsync(loggedUserId);
        var availableChannels = await guild.GetAvailableChannelsAsync(guildUser, true);
        if (availableChannels.Count == 0) return new List<ChannelboardItem>();

        using var context = DbFactory.Create();

        var availableChannelIds = availableChannels.ConvertAll(o => o.Id.ToString());
        var baseQuery = context.UserChannels.AsNoTracking()
            .Where(o => o.Count > 0 && o.GuildId == guild.Id.ToString() && availableChannelIds.Contains(o.ChannelId))
            .GroupBy(o => o.ChannelId)
            .Select(o => new
            {
                ChannelId = o.Key,
                Count = o.Sum(x => x.Count),
                LastMessageAt = o.Max(x => x.LastMessageAt),
                FirstMessageAt = o.Min(x => x.FirstMessageAt)
            });

        var channelStats = await baseQuery.ToListAsync(cancellationToken);
        if (channelStats.Count == 0) return new List<ChannelboardItem>();

        var channelStatIds = channelStats.ConvertAll(o => o.ChannelId);
        var channelsQuery = context.Channels.AsNoTracking()
            .Include(o => o.Guild)
            .Where(o => o.GuildId == guild.Id.ToString() && channelStatIds.Contains(o.ChannelId));

        var result = new List<ChannelboardItem>();
        foreach (var channel in await channelsQuery.ToListAsync(cancellationToken))
        {
            var stats = channelStats.Find(o => o.ChannelId == channel.ChannelId);

            var channelboardItem = Mapper.Map<ChannelboardItem>(channel);
            channelboardItem.Count = stats.Count;
            channelboardItem.LastMessageAt = stats.LastMessageAt;
            channelboardItem.FirstMessageAt = stats.FirstMessageAt;

            result.Add(channelboardItem);
        }

        return result;
    }
}
