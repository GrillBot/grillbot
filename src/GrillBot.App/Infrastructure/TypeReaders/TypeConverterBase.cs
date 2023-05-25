﻿using Discord.Interactions;
using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.TypeReaders;

public abstract class TypeConverterBase<TType> : TypeConverter<TType>
{
    protected static TypeConverterResult FromSuccess(object type)
        => TypeConverterResult.FromSuccess(type);

    protected static TypeConverterResult ParseFailed(IServiceProvider provider, string errorId, string locale)
        => FromError(provider, errorId, InteractionCommandError.ParseFailed, locale);

    protected static TypeConverterResult Unsuccessful(IServiceProvider provider, string errorId, string locale)
        => FromError(provider, errorId, InteractionCommandError.Unsuccessful, locale);

    protected static TypeConverterResult ConvertFailed(IServiceProvider provider, string errorId, string locale)
        => FromError(provider, errorId, InteractionCommandError.ConvertFailed, locale);

    private static TypeConverterResult FromError(IServiceProvider provider, string errorId, InteractionCommandError error, string locale)
    {
        var reason = provider.GetRequiredService<ITextsManager>()[$"TypeConverters/{errorId}", locale];
        return TypeConverterResult.FromError(error, reason);
    }
}
