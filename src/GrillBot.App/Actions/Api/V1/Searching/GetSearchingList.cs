using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class GetSearchingList : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetSearchingList(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<SearchingListItem>> ProcessAsync(GetSearchingListParams parameters)
    {
        var mutualGuilds = await GetMutualGuildsAsync();
        CheckAndSetPublicAccess(parameters, mutualGuilds);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Searching.FindSearchesAsync(parameters, mutualGuilds, parameters.Pagination);
        return await PaginatedResponse<SearchingListItem>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<SearchingListItem>(entity)));
    }

    private void CheckAndSetPublicAccess(GetSearchingListParams parameters, IEnumerable<string> mutualGuilds)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.UserId = ApiContext.GetUserId().ToString();
        if (!string.IsNullOrEmpty(parameters.GuildId) && mutualGuilds.All(o => o != parameters.GuildId))
            parameters.GuildId = null;
    }

    private async Task<List<string>> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic())
            return new List<string>();
        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        return mutualGuilds.ConvertAll(o => o.Id.ToString());
    }
}
