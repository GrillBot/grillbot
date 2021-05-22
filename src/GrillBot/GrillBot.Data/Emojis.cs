using Discord;

namespace GrillBot.Data
{
    static public class Emojis
    {
        public static IEmote MoveToFirst => new Emoji("⏮️");
        public static IEmote MoveToPrev => new Emoji("◀️");
        public static IEmote MoveToNext => new Emoji("▶️");
        public static IEmote MoveToLast => new Emoji("⏭️");
        public static IEmote Ok => new Emoji("✅");

        public static IEmote[] PaginationEmojis => new[] { MoveToFirst, MoveToPrev, MoveToNext, MoveToLast };
    }
}
