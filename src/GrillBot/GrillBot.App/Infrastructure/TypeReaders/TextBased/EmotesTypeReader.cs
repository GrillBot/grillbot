using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.TextBased
{
    public class EmotesTypeReader : TextBasedTypeReader<EmotesConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(EmotesConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeReaderResult.FromSuccess(result);

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Požadovaný emote se nepodařilo najít a současně to není unicode emoji.");
        }
    }
}
