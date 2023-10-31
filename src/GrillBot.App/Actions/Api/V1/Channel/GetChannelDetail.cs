using AutoMapper;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetChannelDetail : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }

    public GetChannelDetail(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IMapper mapper, IDiscordClient discordClient,
        IMessageCacheManager messageCache) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        Mapper = mapper;
        DiscordClient = discordClient;
        MessageCache = messageCache;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (ulong)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(id, null, true, ChannelsIncludeUsersMode.IncludeExceptInactive, true, true)
            ?? throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var result = Mapper.Map<ChannelDetail>(channel);
        if (channel.IsText())
        {
            var threads = await repository.Channel.GetChildChannelsAsync(id);
            result.Threads = Mapper.Map<List<Data.Models.API.Channels.Channel>>(threads);
        }

        if (channel.HasFlag(ChannelFlag.Deleted))
            return ApiResult.Ok(result);

        var guild = await DiscordClient.GetGuildAsync(channel.GuildId.ToUlong(), CacheMode.CacheOnly);
        var guildChannel = guild == null ? null : await guild.GetChannelAsync(id);
        if (guildChannel == null)
            return ApiResult.Ok(result);

        result = Mapper.Map(guildChannel, result);
        if (channel.IsText() || channel.IsThread() || channel.IsVoice())
            result.CachedMessagesCount = await MessageCache.GetCachedMessagesCount(guildChannel);

        return ApiResult.Ok(result);
    }
}
