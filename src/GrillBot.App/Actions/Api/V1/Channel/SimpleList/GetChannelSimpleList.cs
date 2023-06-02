using System.Collections;
using AutoMapper;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Channel.SimpleList;

public class GetChannelSimpleList : SimpleListBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public GetChannelSimpleList(ApiRequestContext apiContext, IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext,
        discordClient, mapper)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<Dictionary<string, string>> ProcessAsync(ulong? guildId, bool noThreads)
    {
        var guilds = await GetGuildsAsync(guildId);
        ValidateParameters(guildId, guilds);

        var channels = await GetAvailableChannelsAsync(guilds, noThreads);
        var mappedChannels = Map(channels);

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
        var guilds = await GetGuildsAsync();
        if (guildId != null)
            guilds = guilds.FindAll(o => o.Id == guildId.Value);
        return guilds;
    }

    private void ValidateParameters(ulong? guildId, ICollection guilds)
    {
        if (guildId != null && ApiContext.IsPublic() && guilds.Count == 0)
            throw new ValidationException(new ValidationResult(Texts["ChannelModule/ChannelSimpleList/NoMutualGuild", ApiContext.Language], new[] { nameof(guildId) }), null, guildId);
    }
}
