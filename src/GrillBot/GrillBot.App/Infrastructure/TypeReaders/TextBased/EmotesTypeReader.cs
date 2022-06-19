using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;

namespace GrillBot.App.Infrastructure.TypeReaders.TextBased;

public class EmotesTypeReader : TextBasedTypeReader<EmotesConverter>
{
    protected override async Task<TypeReaderResult> ProcessAsync(EmotesConverter converter, string input, ICommandContext context, IServiceProvider provider)
    {
        var result = await converter.ConvertAsync(input);

        return result != null
            ? TypeReaderResult.FromSuccess(result)
            : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Požadovaný emote se nepodařilo najít a současně to není unicode emoji.");
    }
}
