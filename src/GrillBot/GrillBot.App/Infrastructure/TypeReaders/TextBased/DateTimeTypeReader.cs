using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.TextBased
{
    public class DateTimeTypeReader : TextBasedTypeReader<DateTimeConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(DateTimeConverter converter, string input, ICommandContext context, IServiceProvider provider)
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
