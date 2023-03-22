using AutoMapper;
using GrillBot.Common.Models;
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

    public async Task<PaginatedResponse<GuildInvite>> ProcessAsync(GetInviteListParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Invite.GetInviteListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<GuildInvite>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<GuildInvite>(entity)));
    }
}
