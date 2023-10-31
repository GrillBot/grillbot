using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;

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

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (long)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id)
            ?? throw new NotFoundException(Texts["AutoReply/NotFound", ApiContext.Language]);

        repository.Remove(entity);
        await repository.CommitAsync();
        await AutoReplyManager.InitAsync();

        return ApiResult.Ok();
    }
}
