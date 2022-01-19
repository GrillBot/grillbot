using Discord.Commands;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased
{
    public class GuidTypeReader : TextBasedTypeReader<Implementations.GuidConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(Implementations.GuidConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeReaderResult.FromSuccess(result);

            return TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaný řetězec není GUID/UUID.");

        }
    }
}
