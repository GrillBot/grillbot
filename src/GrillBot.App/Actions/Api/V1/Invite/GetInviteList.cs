using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Invites;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class GetInviteList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetInviteList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (GetInviteListParams)Parameters[0]!;

        using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Invite.GetInviteListAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<GuildInvite>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<GuildInvite>(entity)));

        return ApiResult.Ok(result);
    }
}
