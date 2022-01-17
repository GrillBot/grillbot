using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.TextBased
{
    public class GuidTypeReader : TextBasedTypeReader<GuidConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(GuidConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeReaderResult.FromSuccess(result);

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaný řetězec není GUID/UUID.");

        }
    }
}
