using Discord;
using Discord.Interactions;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.Interactions
{
    public class BooleanTypeConverter : InteractionsTypeConverter<BooleanConverter, bool>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(BooleanConverter converter, string input, IInteractionContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result == null)
                return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Provided string is not valid boolean value.");

            return TypeConverterResult.FromSuccess(result);
        }
    }
}
