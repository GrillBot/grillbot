using AutoMapper;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class GetAutoReplyItem : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ITextsManager Texts { get; }

    public GetAutoReplyItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        Texts = texts;
    }

    public async Task<AutoReplyItem> ProcessAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(Texts["AutoReply/NotFound", ApiContext.Language].FormatWith(id));

        return Mapper.Map<AutoReplyItem>(entity);
    }
}
