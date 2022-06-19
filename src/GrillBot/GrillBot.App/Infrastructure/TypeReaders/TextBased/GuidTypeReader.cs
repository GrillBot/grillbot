using Discord.Commands;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased;

public class GuidTypeReader : TextBasedTypeReader<Implementations.GuidConverter>
{
    protected override async Task<TypeReaderResult> ProcessAsync(Implementations.GuidConverter converter, string input, ICommandContext context, IServiceProvider provider)
    {
        var result = await converter.ConvertAsync(input);
        return result != null ? TypeReaderResult.FromSuccess(result) : TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaný řetězec není GUID/UUID.");
    }
}
