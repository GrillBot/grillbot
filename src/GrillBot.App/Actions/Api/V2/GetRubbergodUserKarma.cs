using GrillBot.Common.Models;
using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Data.Models.API.Users;
using UserKarma = GrillBot.Common.Services.RubbergodService.Models.Karma.UserKarma;

namespace GrillBot.App.Actions.Api.V2;

public class GetRubbergodUserKarma : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public GetRubbergodUserKarma(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task<PaginatedResponse<UserKarma>> ProcessAsync(KarmaListParams parameters)
    {
        return await RubbergodServiceClient.GetKarmaPageAsync(parameters.Pagination);
    }
}
