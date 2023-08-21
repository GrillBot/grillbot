// ReSharper disable MemberCanBePrivate.Global

using Discord;

namespace GrillBot.Common;

public static class Emojis
{
    public static IEmote MoveToFirst => new Emoji("⏮️");
    public static IEmote MoveToPrev => new Emoji("◀️");
    public static IEmote MoveToNext => new Emoji("▶️");
    public static IEmote MoveToLast => new Emoji("⏭️");
    public static IEmote Ok => new Emoji("✅");
    public static Emoji Nok => new("❌");
    public static Emoji LetterA => new("🇦");
    public static Emoji LetterB => new("🇧");
    public static Emoji LetterC => new("🇨");
    public static Emoji LetterD => new("🇩");
    public static Emoji LetterE => new("🇪");
    public static Emoji LetterF => new("🇫");
    public static Emoji LetterG => new("🇬");
    public static Emoji LetterH => new("🇭");
    public static Emoji LetterI => new("🇮");
    public static Emoji LetterJ => new("🇯");
    public static Emoji LetterK => new("🇰");
    public static Emoji LetterL => new("🇱");
    public static Emoji LetterM => new("🇲");
    public static Emoji LetterN => new("🇳");
    public static Emoji LetterO => new("🇴");
    public static Emoji LetterP => new("🇵");
    public static Emoji LetterQ => new("🇶");
    public static Emoji LetterR => new("🇷");
    public static Emoji LetterS => new("🇸");
    public static Emoji LetterT => new("🇹");
    public static Emoji LetterU => new("🇺");
    public static Emoji LetterV => new("🇻");
    public static Emoji LetterW => new("🇼");
    public static Emoji LetterX => new("🇽");
    public static Emoji LetterY => new("🇾");
    public static Emoji LetterZ => new("🇿");
    public static Emoji SignA => new("🅰");
    public static Emoji SignB => new("🅱");
    public static Emoji SignO => new("🅾");
    public static Emoji Zero => new("0️⃣");
    public static Emoji One => new("1️⃣");
    public static Emoji Two => new("2️⃣");
    public static Emoji Three => new("3️⃣");
    public static Emoji Four => new("4️⃣");
    public static Emoji Five => new("5️⃣");
    public static Emoji Six => new("6️⃣");
    public static Emoji Seven => new("7️⃣");
    public static Emoji Eight => new("8️⃣");
    public static Emoji Nine => new("9️⃣");
    public static Emoji PersonRisingHand => new("🙋");
    public static Emoji EEmail => new("📧");
    public static Emoji Parking => new("🅿️");
    public static Emoji InformationSource => new("ℹ️");
    public static Emoji ThumbsUp => new("👍");
    public static Emoji ThumbsDown => new("👎");
    public static Emoji TrashBin => new("🗑");

    public static Dictionary<int, Emoji> NumberToEmojiMap => new()
    {
        { 0, Zero },
        { 1, One },
        { 2, Two },
        { 3, Three },
        { 4, Four },
        { 5, Five },
        { 6, Six },
        { 7, Seven },
        { 8, Eight },
        { 9, Nine }
    };

    public static Dictionary<char, Emoji> CharToEmojiMap => new()
    {
        { 'A', LetterA },
        { 'B', LetterB },
        { 'C', LetterC },
        { 'D', LetterD },
        { 'E', LetterE },
        { 'F', LetterF },
        { 'G', LetterG },
        { 'H', LetterH },
        { 'I', LetterI },
        { 'J', LetterJ },
        { 'K', LetterK },
        { 'L', LetterL },
        { 'M', LetterM },
        { 'N', LetterN },
        { 'O', LetterO },
        { 'P', LetterP },
        { 'Q', LetterQ },
        { 'R', LetterR },
        { 'S', LetterS },
        { 'T', LetterT },
        { 'U', LetterU },
        { 'V', LetterV },
        { 'W', LetterW },
        { 'X', LetterX },
        { 'Y', LetterY },
        { 'Z', LetterZ }
    };

    public static Dictionary<char, Emoji> CharToSignEmojiMap => new()
    {
        { 'A', SignA },
        { 'B', SignB },
        { 'O', SignO },
        { 'E', EEmail },
        { 'P', Parking },
        { 'I', InformationSource },
        { 'X', Nok }
    };

    public static Emoji? ConvertCharacterToEmoji(char character, bool alternativeFirst = false)
    {
        if (char.IsDigit(character) && NumberToEmojiMap.ContainsKey((int)char.GetNumericValue(character)))
            return NumberToEmojiMap[(int)char.GetNumericValue(character)];

        if (alternativeFirst && CharToSignEmojiMap.ContainsKey(character))
            return CharToSignEmojiMap[character];

        return CharToEmojiMap.ContainsKey(character) ? CharToEmojiMap[character] : null;
    }

    public static IEnumerable<IEmote> VoteEmojis => new[] { ThumbsUp, ThumbsDown };
}
