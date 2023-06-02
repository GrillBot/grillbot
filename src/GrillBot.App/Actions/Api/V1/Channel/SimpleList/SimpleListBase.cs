using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Channel.SimpleList;

public abstract class SimpleListBase : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    protected IMapper Mapper { get; }

    private ulong UserId
        => ApiContext.GetUserId();

    protected SimpleListBase(ApiRequestContext apiContext, IDiscordClient discordClient, IMapper mapper) : base(apiContext)
    {
        DiscordClient = discordClient;
        Mapper = mapper;
    }

    protected async Task<List<IGuild>> GetGuildsAsync()
    {
        return ApiContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(UserId) : (await DiscordClient.GetGuildsAsync()).ToList();
    }

    protected async Task<List<IGuildChannel>> GetAvailableChannelsAsync(List<IGuild> guilds, bool noThreads)
    {
        var result = new List<IGuildChannel>();

        foreach (var guild in guilds)
            result.AddRange(await GetAvailableChannelsAsync(guild, noThreads));

        return result;
    }

    private async Task<List<IGuildChannel>> GetAvailableChannelsAsync(IGuild guild, bool noThreads)
    {
        if (ApiContext.IsPublic())
        {
            var guildUser = await guild.GetUserAsync(UserId);
            return await guild.GetAvailableChannelsAsync(guildUser, noThreads);
        }

        var channels = await guild.GetChannelsAsync();
        return (noThreads ? channels.Where(o => o is not IThreadChannel) : channels).ToList();
    }

    protected static Dictionary<string, string> CreateResult(IEnumerable<Data.Models.API.Channels.Channel> channels)
    {
        return channels
            .DistinctBy(o => o.Id)
            .OrderBy(o => o.Name)
            .ToDictionary(o => o.Id, o => $"{o.Name} {(o.Type is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread ? "(Thread)" : "")}".Trim());
    }

    protected List<Data.Models.API.Channels.Channel> Map(IEnumerable<IGuildChannel> channels)
    {
        return Mapper.Map<List<Data.Models.API.Channels.Channel>>(channels)
            .FindAll(o => o.Type is not null && o.Type != ChannelType.Category && o.Type != ChannelType.DM);
    }
}
