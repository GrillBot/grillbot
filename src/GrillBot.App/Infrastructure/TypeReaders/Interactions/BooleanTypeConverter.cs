using Discord.Interactions;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class BooleanTypeConverter : InteractionsTypeConverter<Implementations.BooleanConverter, bool>
{
    protected override async Task<TypeConverterResult> ProcessAsync(Implementations.BooleanConverter converter, string input, IInteractionContext context, IServiceProvider provider,
        ITextsManager texts)
    {
        var result = await converter.ConvertAsync(input);

        return result == null
            ? TypeConverterResult.FromError(InteractionCommandError.ParseFailed, texts["TypeConverter/BooleanInvalidValue", context.Interaction.UserLocale])
            : TypeConverterResult.FromSuccess(result);
    }
}
