using AutoMapper;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Channel.SimpleList;

/// <summary>
/// Gets list of channels that contains pins.
/// </summary>
public class GetChannelSimpleListWithPins : SimpleListBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetChannelSimpleListWithPins(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext, discordClient, mapper)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<Dictionary<string, string>> ProcessAsync()
    {
        var guilds = await GetGuildsAsync();
        var availableChannels = await GetAvailableChannelsAsync(guilds, false);

        await using var repository = DatabaseBuilder.CreateRepository();
        var channelsWithPins = await repository.Channel.GetChannelsWithPinsAsync(guilds);

        var filteredChannels = availableChannels.FindAll(o =>
            channelsWithPins.Exists(x => x.GuildId == o.GuildId.ToString() && x.ChannelId == o.Id.ToString())
        );

        return CreateResult(Map(filteredChannels));
    }
}
