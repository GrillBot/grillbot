using Discord;
using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions
{
    public class DateTimeTypeConverter : InteractionsTypeConverter<DateTimeConverter, DateTime>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(DateTimeConverter converter, string input, IInteractionContext context, IServiceProvider provider)
        {
            try
            {
                var result = await converter.ConvertAsync(input);
                return TypeConverterResult.FromSuccess(result);
            }
            catch (InvalidOperationException ex)
            {
                return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, ex.Message);
            }
        }
    }
}
