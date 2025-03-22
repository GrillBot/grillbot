using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class CreateAutoReplyItem : ApiAction
{
    private AutoReplyManager AutoReplyManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public CreateAutoReplyItem(ApiRequestContext apiContext, AutoReplyManager autoReplyManager, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        AutoReplyManager = autoReplyManager;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (AutoReplyItemParams)Parameters[0]!;
        var entity = new Database.Entity.AutoReplyItem
        {
            Flags = parameters.Flags,
            Reply = parameters.Reply,
            Template = parameters.Template
        };

        var repository = DatabaseBuilder.CreateRepository();

        await repository.AddAsync(entity);
        await repository.CommitAsync();
        await AutoReplyManager.InitAsync();

        var result = Mapper.Map<AutoReplyItem>(entity);
        return ApiResult.Ok(result);
    }
}
