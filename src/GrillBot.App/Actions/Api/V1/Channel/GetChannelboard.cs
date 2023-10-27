using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Channels;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetChannelboard : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetChannelboard(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        var result = new List<ChannelboardItem>();

        foreach (var guild in mutualGuilds)
            result.AddRange(await GetChannelboardAsync(guild));

        result = result.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).ToList();
        return ApiResult.Ok(result);
    }

    private async Task<List<ChannelboardItem>> GetChannelboardAsync(IGuild guild)
    {
        var result = new List<ChannelboardItem>();
        var loggedGuildUser = await guild.GetUserAsync(ApiContext.GetUserId());
        var availableChannels = await guild.GetAvailableChannelsAsync(loggedGuildUser, true, false);
        if (availableChannels.Count == 0) return result;

        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Channel.GetAvailableStatsAsync(guild, availableChannels.Select(o => o.Id.ToString()), true);
        if (statistics.Count == 0) return result;

        var channels = await repository.Channel.GetVisibleChannelsAsync(guild.Id, statistics.Keys.ToList(), true, true);
        foreach (var channel in channels)
        {
            var (count, firstMessageAt, lastMessageAt) = statistics[channel.ChannelId];
            var channelboardItem = Mapper.Map<ChannelboardItem>(channel);

            channelboardItem.Count = count;
            channelboardItem.LastMessageAt = lastMessageAt;
            channelboardItem.FirstMessageAt = firstMessageAt;
            result.Add(channelboardItem);
        }

        return result;
    }
}
