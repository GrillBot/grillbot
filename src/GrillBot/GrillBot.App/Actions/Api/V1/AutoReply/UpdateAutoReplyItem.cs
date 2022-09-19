using AutoMapper;
using GrillBot.App.Services;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class UpdateAutoReplyItem : ApiAction
{
    private AutoReplyService Service { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ITextsManager Texts { get; }

    public UpdateAutoReplyItem(ApiRequestContext apiContext, AutoReplyService service, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ITextsManager texts) : base(apiContext)
    {
        Service = service;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        Texts = texts;
    }

    public async Task<(AutoReplyItem item, string errMsg)> ProcessAsync(long id, AutoReplyItemParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            return (null, Texts["AutoReply/NotFound", ApiContext.Language].FormatWith(id));

        entity.Template = parameters.Template;
        entity.Flags = parameters.Flags;
        entity.Reply = parameters.Reply;

        await repository.CommitAsync();
        await Service.InitAsync();

        return (Mapper.Map<AutoReplyItem>(entity), null);
    }
}
