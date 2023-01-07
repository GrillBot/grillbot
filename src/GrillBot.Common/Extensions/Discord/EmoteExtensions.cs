using NeoSmart.Unicode;

namespace GrillBot.Common.Extensions.Discord;

public static class EmoteExtensions
{
    public static bool IsEqual(this global::Discord.IEmote emote, global::Discord.IEmote another)
    {
        if (!emote.GetType().IsInstanceOfType(another) && !another.GetType().IsInstanceOfType(emote))
            return false;

        // In a case of standard emotes.
        if (emote is not global::Discord.Emoji emoji)
            return emote.Equals(another) && emote.Name == another.Name;

        var emojiCodepoint = emoji.Name.Codepoints().FirstOrDefault();
        var anotherEmojiCodepoint = another.Name.Codepoints().FirstOrDefault();

        return emojiCodepoint == anotherEmojiCodepoint;
    }
}
