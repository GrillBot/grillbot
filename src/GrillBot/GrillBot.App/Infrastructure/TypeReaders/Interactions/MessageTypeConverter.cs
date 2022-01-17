using Discord;
using Discord.Interactions;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using GrillBot.Data.Exceptions;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.Interactions
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
