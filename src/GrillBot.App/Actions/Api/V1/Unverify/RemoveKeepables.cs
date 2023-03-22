﻿using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveKeepables : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public RemoveKeepables(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(string group, string? name = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        if (string.IsNullOrEmpty(name))
        {
            if (!await repository.SelfUnverify.KeepableExistsAsync(group))
                ThrowValidationException("GroupNotExists", group, group);

            var items = await repository.SelfUnverify.GetKeepablesAsync(group, true);
            if (items.Count > 0)
                repository.RemoveCollection(items);
        }
        else
        {
            if (!await repository.SelfUnverify.KeepableExistsAsync(group, name))
                ThrowValidationException("NotExists", $"{group}/{name}", group, name);

            var item = await repository.SelfUnverify.FindKeepableAsync(group, name);
            if (item != null)
                repository.Remove(item);
        }

        await repository.CommitAsync();
    }

    private void ThrowValidationException(string errorId, object value, params object[] args)
    {
        throw new ValidationException(new ValidationResult(Texts[$"Unverify/SelfUnverify/Keepables/{errorId}", ApiContext.Language].FormatWith(args)), null, value);
    }
}
