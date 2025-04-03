using AutoMapper;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.InviteService;
using GrillBot.Core.Services.InviteService.Models.Request;
using GrillBot.Data.Models.API.Invites;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class GetInviteList(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IInviteServiceClient> _client,
    DataResolveManager _dataResolve
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetInviteListParams)Parameters[0]!;
        var request = CreateRequest(parameters);

        var response = await _client.ExecuteRequestAsync((client, cancellationToken) => client.GetUsedInvitesAsync(request, cancellationToken));

        var result = await PaginatedResponse<GuildInvite>.CopyAndMapAsync(response, async entity =>
        {
            return new GuildInvite
            {
                Code = entity.Code,
                CreatedAt = entity.CreatedAt?.ToLocalTime(),
                Creator = string.IsNullOrEmpty(entity.CreatorId) ? null : await _dataResolve.GetUserAsync(entity.CreatorId.ToUlong()),
                Guild = (await _dataResolve.GetGuildAsync(entity.GuildId.ToUlong()))!,
                UsedUsersCount = entity.Uses
            };
        });

        return ApiResult.Ok(result);
    }

    private static InviteListRequest CreateRequest(GetInviteListParams parameters)
    {
        return new InviteListRequest
        {
            Code = parameters.Code,
            GuildId = parameters.GuildId,
            CreatorId = parameters.CreatorId,
            CreatedFrom = parameters.CreatedFrom,
            CreatedTo = parameters.CreatedTo,
            Pagination = parameters.Pagination,
            Sort = new Core.Models.SortParameters
            {
                OrderBy = parameters.Sort.OrderBy,
                Descending = parameters.Sort.Descending,
            },
        };
    }
}
