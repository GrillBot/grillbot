using AutoMapper;
using GrillBot.Common.Models;
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

    public async Task<List<AutoReplyItem>> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var items = await repository.AutoReply.GetAllAsync();
        return Mapper.Map<List<AutoReplyItem>>(items);
    }
}
