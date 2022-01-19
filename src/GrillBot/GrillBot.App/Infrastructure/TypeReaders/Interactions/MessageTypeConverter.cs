using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions
{
    public class MessageTypeConverter : InteractionsTypeConverter<MessageConverter, IMessage>
    {
        protected override async Task<TypeConverterResult> ProcessAsync(MessageConverter converter, string input, IInteractionContext context, IServiceProvider provider)
        {
            try
            {
                var result = await converter.ConvertAsync(input);
                return TypeConverterResult.FromSuccess(result);
            }
            catch (UriFormatException ex)
            {
                return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, ex.Message);
            }
            catch (FormatException ex)
            {
                return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return TypeConverterResult.FromError(InteractionCommandError.Unsuccessful, ex.Message);
            }
            catch (NotFoundException ex)
            {
                return TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, ex.Message);
            }
        }
    }
}
