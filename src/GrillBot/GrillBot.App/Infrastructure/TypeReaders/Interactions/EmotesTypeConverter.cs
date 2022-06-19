using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class EmotesTypeConverter : InteractionsTypeConverter<EmotesConverter, IEmote>
{
    protected override async Task<TypeConverterResult> ProcessAsync(EmotesConverter converter, string input, IInteractionContext context, IServiceProvider provider)
    {
        var result = await converter.ConvertAsync(input);

        return result != null
            ? TypeConverterResult.FromSuccess(result)
            : TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Požadovaný emote se nepodařilo najít a současně to není unicode emoji.");
    }
}
