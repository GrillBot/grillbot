using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class RemoveAutoReplyItem : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private AutoReplyManager AutoReplyManager { get; }

    public RemoveAutoReplyItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, AutoReplyManager autoReplyManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        AutoReplyManager = autoReplyManager;
    }

    public async Task ProcessAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(Texts["AutoReply/NotFound", ApiContext.Language]);

        repository.Remove(entity);
        await repository.CommitAsync();
        await AutoReplyManager.InitAsync();
    }
}
