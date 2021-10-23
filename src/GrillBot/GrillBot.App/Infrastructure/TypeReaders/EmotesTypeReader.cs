using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class EmotesTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (NeoSmart.Unicode.Emoji.IsEmoji(input))
                return TypeReaderResult.FromSuccess(new Emoji(input));

            if (Emote.TryParse(input, out Emote emote))
                return TypeReaderResult.FromSuccess(emote);

            if (context.Guild != null)
            {
                if (ulong.TryParse(input, out ulong emoteId))
                {
                    emote = await context.Guild.GetEmoteAsync(emoteId);

                    if (emote != null)
                        return TypeReaderResult.FromSuccess(emote);
                }

                emote = context.Guild.Emotes.FirstOrDefault(o => o.Name == input);
            }

            if (emote != null)
                return TypeReaderResult.FromSuccess(emote);

            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Požadovaný emote se nepodařilo najít a současně to není unicode emoji.");
        }
    }
}
