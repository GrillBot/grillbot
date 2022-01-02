using Discord;
using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions
{
    public class GuidTypeConverter : InteractionsTypeConverter<GuidConverter, Guid>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(GuidConverter converter, string input, IInteractionContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeConverterResult.FromSuccess(result);

            return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Zadaný řetězec není GUID/UUID.");
        }
    }
}
