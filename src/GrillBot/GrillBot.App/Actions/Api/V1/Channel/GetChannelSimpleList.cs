using System.Collections;
using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetChannelSimpleList : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public GetChannelSimpleList(ApiRequestContext apiContext, IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<Dictionary<string, string>> ProcessAsync(ulong? guildId, bool noThreads)
    {
        var guilds = await GetGuildsAsync(guildId);
        ValidateParameters(guildId, guilds);

        var channels = await GetAvailableChannelsAsync(guilds, noThreads);
        var mappedChannels = Mapper.Map<List<Data.Models.API.Channels.Channel>>(channels)
            .FindAll(o => o.Type != null && o.Type != ChannelType.Category);

        if (ApiContext.IsPublic())
            return CreateResult(mappedChannels);

        var guildIds = guilds.ConvertAll(o => o.Id.ToString());
        await using var repository = DatabaseBuilder.CreateRepository();

        var databaseChannels = await repository.Channel.GetAllChannelsAsync(guildIds, noThreads, true);
        databaseChannels = databaseChannels.FindAll(o => mappedChannels.All(x => x.Id != o.ChannelId));
        mappedChannels.AddRange(Mapper.Map<List<Data.Models.API.Channels.Channel>>(databaseChannels));

        return CreateResult(mappedChannels);
    }

    private async Task<List<IGuild>> GetGuildsAsync(ulong? guildId)
    {
        var guilds = ApiContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()) : (await DiscordClient.GetGuildsAsync()).ToList();
        if (guildId != null)
            guilds = guilds.FindAll(o => o.Id == guildId.Value);
        return guilds;
    }

    private async Task<List<IGuildChannel>> GetAvailableChannelsAsync(List<IGuild> guilds, bool noThreads)
    {
        var result = new List<IGuildChannel>();

        foreach (var guild in guilds)
        {
            if (ApiContext.IsPublic())
            {
                var guildUser = await guild.GetUserAsync(ApiContext.GetUserId());
                result.AddRange(await guild.GetAvailableChannelsAsync(guildUser, noThreads));
            }
            else
            {
                var channels = await guild.GetChannelsAsync();
                if (noThreads)
                    channels = channels.Where(o => o is not IThreadChannel).ToList().AsReadOnly();
                result.AddRange(channels);
            }
        }

        return result;
    }

    private void ValidateParameters(ulong? guildId, ICollection guilds)
    {
        if (guildId != null && ApiContext.IsPublic() && guilds.Count == 0)
            throw new ValidationException(new ValidationResult(Texts["ChannelModule/ChannelSimpleList/NoMutualGuild", ApiContext.Language], new[] { nameof(guildId) }), null, guildId);
    }

    private static Dictionary<string, string> CreateResult(IEnumerable<Data.Models.API.Channels.Channel> channels)
    {
        return channels.DistinctBy(o => o.Id)
            .OrderBy(o => o.Name)
            .ToDictionary(o => o.Id, o => $"{o.Name} {(o.Type is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread ? " (Thread)" : "")}".Trim());
    }
}
