using Discord.Commands;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased
{
    public class DateTimeTypeReader : TextBasedTypeReader<Implementations.DateTimeConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(Implementations.DateTimeConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            try
            {
                var result = await converter.ConvertAsync(input);
                return TypeReaderResult.FromSuccess(result);
            }
            catch (InvalidOperationException ex)
            {
                return TypeReaderResult.FromError(CommandError.ParseFailed, ex.Message);
            }
        }
    }
}
