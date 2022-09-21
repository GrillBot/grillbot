using AutoMapper;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetChannelList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }
    private IMapper Mapper { get; }

    public GetChannelList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IMessageCacheManager messageCache,
        IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        MessageCache = messageCache;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<GuildChannelListItem>> ProcessAsync(GetChannelListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channels = await repository.Channel.GetChannelListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<GuildChannelListItem>.CopyAndMapAsync(channels, MapAsync);
    }

    private async Task<GuildChannelListItem> MapAsync(Database.Entity.GuildChannel entity)
    {
        var guild = await DiscordClient.GetGuildAsync(entity.GuildId.ToUlong(), CacheMode.CacheOnly);
        var channel = guild == null ? null : await guild.GetChannelAsync(entity.ChannelId.ToUlong());

        var result = Mapper.Map<GuildChannelListItem>(entity);
        if (channel != null)
        {
            result = Mapper.Map(channel, result);

            if (entity.IsText() || entity.IsThread() || entity.IsVoice())
                result.CachedMessagesCount = await MessageCache.GetCachedMessagesCount(channel);
        }

        if (result.FirstMessageAt == DateTime.MinValue) result.FirstMessageAt = null;
        if (result.LastMessageAt == DateTime.MinValue) result.LastMessageAt = null;
        return result;
    }
}
