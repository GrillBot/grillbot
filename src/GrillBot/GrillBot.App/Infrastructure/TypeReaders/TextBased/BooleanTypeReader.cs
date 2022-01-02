using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased
{
    public class BooleanTypeReader : TextBasedTypeReader<BooleanConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(BooleanConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result == null)
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Provided string is not valid boolean value.");

            return TypeReaderResult.FromSuccess(result);
        }
    }
}
