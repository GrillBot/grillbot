using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common.Executor;
using SearchingService;
using SearchingService.Models.Request;
using GrillBot.Data.Models.API.Searching;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class GetSearchingList(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    IServiceClientExecutor<ISearchingServiceClient> _searchingService,
    DataResolveManager _dataResolve
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetSearchingListParams)Parameters[0]!;
        var result = await ProcessAsync(parameters);

        return ApiResult.Ok(result);
    }

    public async Task<PaginatedResponse<SearchingListItem>> ProcessAsync(GetSearchingListParams parameters)
    {
        var mutualGuilds = await GetMutualGuildsAsync();
        CheckAndSetPublicAccess(parameters, mutualGuilds);

        var request = new SearchingListRequest
        {
            ChannelId = parameters.ChannelId,
            GuildId = parameters.GuildId,
            HideInvalid = true,
            MessageQuery = parameters.MessageQuery,
            Pagination = parameters.Pagination,
            ShowDeleted = false,
            Sort = new()
            {
                Descending = parameters.Sort.Descending,
                OrderBy = parameters.Sort.OrderBy
            },
            UserId = parameters.UserId
        };

        var response = await _searchingService.ExecuteRequestAsync((c, ctx) => c.GetSearchingListAsync(request, ctx.CancellationToken));
        if (mutualGuilds is not null)
            response.Data = response.Data.FindAll(o => mutualGuilds.Contains(o.GuildId));

        return await PaginatedResponse<SearchingListItem>.CopyAndMapAsync(response, async entity => new SearchingListItem
        {
            Channel = (await _dataResolve.GetChannelAsync(entity.GuildId.ToUlong(), entity.ChannelId.ToUlong()))!,
            Guild = (await _dataResolve.GetGuildAsync(entity.GuildId.ToUlong()))!,
            Id = entity.Id,
            Message = entity.Content,
            User = (await _dataResolve.GetUserAsync(entity.UserId.ToUlong()))!
        });
    }

    private void CheckAndSetPublicAccess(GetSearchingListParams parameters, IEnumerable<string>? mutualGuilds)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.UserId = ApiContext.GetUserId().ToString();
        if (!string.IsNullOrEmpty(parameters.GuildId) && mutualGuilds?.All(o => o != parameters.GuildId) == true)
            parameters.GuildId = null;
    }

    private async Task<List<string>?> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic())
            return null;

        var mutualGuilds = await _discordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        return mutualGuilds.ConvertAll(o => o.Id.ToString());
    }
}
