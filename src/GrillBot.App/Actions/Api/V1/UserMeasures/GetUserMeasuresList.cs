using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.UserMeasures;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.MeasuresList;
using GrillBot.Data.Enums;
using GrillBot.App.Managers.DataResolve;

namespace GrillBot.App.Actions.Api.V1.UserMeasures;

public class GetUserMeasuresList : ApiAction
{
    private IUserMeasuresServiceClient UserMeasuresService { get; }
    private readonly DataResolveManager _dataResolveManager;

    public GetUserMeasuresList(ApiRequestContext apiContext, IUserMeasuresServiceClient userMeasuresService, DataResolveManager dataResolveManager) : base(apiContext)
    {
        UserMeasuresService = userMeasuresService;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (MeasuresListParams)Parameters[0]!;

        if (parameters.CreatedFrom.HasValue)
            parameters.CreatedFrom = parameters.CreatedFrom.Value.WithKind(DateTimeKind.Local).ToUniversalTime();
        if (parameters.CreatedTo.HasValue)
            parameters.CreatedTo = parameters.CreatedTo.Value.WithKind(DateTimeKind.Local).ToUniversalTime();

        var measures = await UserMeasuresService.GetMeasuresListAsync(parameters);
        measures.ValidationErrors.AggregateAndThrow();

        var result = await PaginatedResponse<UserMeasuresListItem>.CopyAndMapAsync(measures.Response!, MapAsync);
        return ApiResult.Ok(result);
    }

    private async Task<UserMeasuresListItem> MapAsync(MeasuresItem entity)
    {
        var guild = await _dataResolveManager.GetGuildAsync(entity.GuildId.ToUlong());
        var moderator = await _dataResolveManager.GetUserAsync(entity.ModeratorId.ToUlong());
        var user = await _dataResolveManager.GetUserAsync(entity.UserId.ToUlong());

        return new UserMeasuresListItem
        {
            CreatedAt = entity.CreatedAtUtc.ToLocalTime(),
            Guild = guild!,
            Moderator = moderator!,
            Reason = entity.Reason,
            Type = entity.Type switch
            {
                "Warning" => UserMeasuresType.Warning,
                "Timeout" => UserMeasuresType.Timeout,
                "Unverify" => UserMeasuresType.Unverify,
                _ => 0
            },
            User = user!,
            ValidTo = entity.ValidTo
        };
    }
}
