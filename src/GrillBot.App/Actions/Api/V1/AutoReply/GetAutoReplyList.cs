using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class GetAutoReplyList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetAutoReplyList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        using var repository = DatabaseBuilder.CreateRepository();

        var items = await repository.AutoReply.GetAllAsync(false);
        var result = Mapper.Map<List<AutoReplyItem>>(items);

        return ApiResult.Ok(result);
    }
}
