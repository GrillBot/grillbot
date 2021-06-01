using NeoSmart.Unicode;
using System.Linq;

namespace GrillBot.App.Extensions.Discord
{
    static public class EmoteExtensions
    {
        static public bool IsEqual(this global::Discord.IEmote emote, global::Discord.IEmote another)
        {
            if (emote.GetType().IsInstanceOfType(another))
                return false;

            if (emote is global::Discord.Emoji emoji)
            {
                var emojiCodepoint = emoji.Name.Codepoints().FirstOrDefault();
                var anotherEmojiCodepoint = another.Name.Codepoints().FirstOrDefault();

                return emojiCodepoint == anotherEmojiCodepoint;
            }

            // In a case of standard emotes.
            return emote.Equals(another);
        }
    }
}
