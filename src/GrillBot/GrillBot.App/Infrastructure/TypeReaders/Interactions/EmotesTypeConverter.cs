using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class EmotesTypeConverter : InteractionsTypeConverter<EmotesConverter, IEmote>
{
    protected override async Task<TypeConverterResult> ProcessAsync(EmotesConverter converter, string input, IInteractionContext context, IServiceProvider provider, ITextsManager texts)
    {
        var result = await converter.ConvertAsync(input);

        return result != null
            ? TypeConverterResult.FromSuccess(result)
            : TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, texts["TypeConverters/EmoteInvalidFormat", context.Interaction.UserLocale]);
    }
}
