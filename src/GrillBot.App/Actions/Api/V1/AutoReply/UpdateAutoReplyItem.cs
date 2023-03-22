﻿using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Actions.Api.V1.AutoReply;

public class UpdateAutoReplyItem : ApiAction
{
    private AutoReplyManager AutoReplyManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ITextsManager Texts { get; }

    public UpdateAutoReplyItem(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, ITextsManager texts, AutoReplyManager autoReplyManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        Texts = texts;
        AutoReplyManager = autoReplyManager;
    }

    public async Task<AutoReplyItem> ProcessAsync(long id, AutoReplyItemParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(Texts["AutoReply/NotFound", ApiContext.Language].FormatWith(id));

        entity.Template = parameters.Template;
        entity.Flags = parameters.Flags;
        entity.Reply = parameters.Reply;

        await repository.CommitAsync();
        await AutoReplyManager.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }
}
