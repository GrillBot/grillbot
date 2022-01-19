using Discord.Interactions;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions
{
    public class DateTimeTypeConverter : InteractionsTypeConverter<Implementations.DateTimeConverter, DateTime>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(Implementations.DateTimeConverter converter, string input, IInteractionContext context, IServiceProvider provider)
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
