using GrillBot.App.Services;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class RemoveAutoReplyItem : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private AutoReplyService Service { get; }

    public RemoveAutoReplyItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, AutoReplyService service) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        Service = service;
    }

    public async Task ProcessAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(Texts["AutoReply/NotFound", ApiContext.Language]);

        repository.Remove(entity);
        await repository.CommitAsync();
        await Service.InitAsync();
    }
}
