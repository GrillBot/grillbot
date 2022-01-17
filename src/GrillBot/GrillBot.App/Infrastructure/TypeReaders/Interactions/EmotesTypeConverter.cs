using Discord;
using Discord.Interactions;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.Interactions
{
    public class EmotesTypeConverter : InteractionsTypeConverter<EmotesConverter, IEmote>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(EmotesConverter converter, string input, IInteractionContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeConverterResult.FromSuccess(result);

            return TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Požadovaný emote se nepodařilo najít a současně to není unicode emoji.");
        }
    }
}
