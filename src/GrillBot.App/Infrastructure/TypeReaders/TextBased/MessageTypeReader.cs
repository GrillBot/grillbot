using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased;

public class MessageTypeReader : TextBasedTypeReader<MessageConverter>
{
    protected override async Task<TypeReaderResult> ProcessAsync(MessageConverter converter, string input, ICommandContext context, IServiceProvider provider)
    {
        try
        {
            var result = await converter.ConvertAsync(input);
            return TypeReaderResult.FromSuccess(result);
        }
        catch (UriFormatException ex)
        {
            return TypeReaderResult.FromError(CommandError.ParseFailed, ex.Message);
        }
        catch (FormatException ex)
        {
            return TypeReaderResult.FromError(CommandError.ParseFailed, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return TypeReaderResult.FromError(CommandError.Unsuccessful, ex.Message);
        }
        catch (NotFoundException ex)
        {
            return TypeReaderResult.FromError(CommandError.ObjectNotFound, ex.Message);
        }
    }
}
