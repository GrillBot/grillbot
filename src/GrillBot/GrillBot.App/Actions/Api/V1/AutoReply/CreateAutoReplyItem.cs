using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Models;
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

    public async Task<AutoReplyItem> ProcessAsync(AutoReplyItemParams parameters)
    {
        var entity = new Database.Entity.AutoReplyItem
        {
            Flags = parameters.Flags,
            Reply = parameters.Reply,
            Template = parameters.Template
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.AddAsync(entity);
        await repository.CommitAsync();
        await AutoReplyManager.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }
}
