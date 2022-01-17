using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.Implementations;

public class EmotesConverter : ConverterBase<IEmote>
{
    public EmotesConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public EmotesConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    public override async Task<IEmote> ConvertAsync(string value)
    {
        if (NeoSmart.Unicode.Emoji.IsEmoji(value)) return new Emoji(value);
        if (Emote.TryParse(value, out Emote emote)) return emote;

        if (Guild != null)
        {
            if (ulong.TryParse(value, out ulong emoteId))
            {
                emote = await Guild.GetEmoteAsync(emoteId);

                if (emote != null) return emote;
            }

            emote = Guild.Emotes.FirstOrDefault(o => o.Name == value);
        }

        return emote;
    }
}
