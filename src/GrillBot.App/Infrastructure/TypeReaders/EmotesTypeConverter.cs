﻿using Discord.Interactions;
using Discord.Net;

namespace GrillBot.App.Infrastructure.TypeReaders;

public class EmotesTypeConverter : TypeConverterBase<IEmote>
{
    public override ApplicationCommandOptionType GetDiscordType()
        => ApplicationCommandOptionType.String;

    public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var value = (string)option.Value;

        if (NeoSmart.Unicode.Emoji.IsEmoji(value))
            return FromSuccess(new Emoji(value));
        if (Emote.TryParse(value, out var emote))
            return FromSuccess(emote);

        if (ulong.TryParse(value, out var emoteId))
        {
            emote = await TryDownloadEmoteAsync(context.Guild, emoteId);

            if (emote != null)
                return FromSuccess(emote);
        }

        emote = context.Guild.Emotes.FirstOrDefault(o => o.Name == value);
        return emote is not null ? FromSuccess(emote) : ConvertFailed(services, "EmoteInvalidFormat", context.Interaction.UserLocale);
    }

    private static async Task<Emote?> TryDownloadEmoteAsync(IGuild guild, ulong emoteId)
    {
        try
        {
            return await guild.GetEmoteAsync(emoteId);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownEmoji)
        {
            return null;
        }
    }
}
