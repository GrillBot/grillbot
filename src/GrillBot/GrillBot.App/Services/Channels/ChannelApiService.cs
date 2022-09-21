using AutoMapper;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;

namespace GrillBot.App.Services.Channels;

public class ChannelApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public ChannelApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient client, ApiRequestContext apiRequestContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = client;
        ApiRequestContext = apiRequestContext;
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
