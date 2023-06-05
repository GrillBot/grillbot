using GrillBot.Common;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class Emojization : CommandAction
{
    private ITextsManager Texts { get; }

    public Emojization(ITextsManager texts)
    {
        Texts = texts;
    }

    public string Process(string message)
    {
        var tokens = CheckAndParseContent(message);
        var emotes = ConvertTokensToEmotes(tokens, true);
        emotes = FilterUnknownEmotes(emotes);
        emotes = AddSpaces(emotes).ToList();
        EnsureNotEmpty(emotes);

        return BuildMessage(emotes);
    }

    public IEnumerable<IEmote> ProcessForReacts(string message, int maxCount)
    {
        var tokens = CheckAndParseContent(message);
        var emotes = ConvertTokensToEmotes(tokens, false).ToList();
        emotes = FilterUnknownEmotes(emotes);
        emotes = emotes.Where(o => o is not null).Take(maxCount).ToList();
        EnsureNotEmpty(emotes);

        return emotes!;
    }

    private List<object> CheckAndParseContent(string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ValidationException(Texts["Emojization/NoContent", Locale]);

        return MessageHelper.ParseMessage(message).ToList();
    }

    private List<IEmote?> ConvertTokensToEmotes(List<object> tokens, bool allowDuplicity)
    {
        var result = new List<IEmote?>();

        void AddEmoteFromChar(char @char)
        {
            if (@char == ' ')
            {
                result.Add(null);
                return;
            }

            @char = char.ToUpper(@char);
            var emote = Emojis.ConvertCharacterToEmoji(@char);
            if ((emote != null && result.Contains(emote) && !allowDuplicity) || emote == null)
                emote = Emojis.ConvertCharacterToEmoji(@char, true);

            if (emote == null) return;
            if (result.Contains(emote) && !allowDuplicity)
                throw new ValidationException(Texts["Emojization/DuplicateChar", Locale].FormatWith(emote.ToString()));
            result.Add(emote);
        }

        foreach (var token in tokens)
        {
            switch (token)
            {
                case IEmote emote:
                    if (!allowDuplicity && result.Any(x => Equals(x, emote)))
                        throw new ValidationException(Texts["Emojization/DuplicateChar", Locale].FormatWith(emote.ToString()));
                    result.Add(emote);
                    break;
                case char @char:
                    AddEmoteFromChar(@char);
                    break;
                case string str:
                {
                    foreach (var stringChar in str)
                        AddEmoteFromChar(stringChar);
                    break;
                }
            }
        }

        return result;
    }

    private void EnsureNotEmpty(IReadOnlyCollection<IEmote?> emotes)
    {
        var isEmpty = emotes.Count == 0 || emotes.All(o => o is null);
        if (isEmpty)
            throw new ValidationException(Texts["Emojization/EmptyResult", Locale]);
    }

    private static IEnumerable<IEmote?> AddSpaces(IEnumerable<IEmote?> emotes)
    {
        foreach (var emote in emotes)
        {
            yield return emote;
            yield return null;
        }
    }

    private static string BuildMessage(IEnumerable<IEmote?> emotes)
    {
        var builder = new StringBuilder();

        void AppendValue(string str)
        {
            if (builder.Length + str.Length > DiscordConfig.MaxMessageSize)
                return;
            if (str == " " && (builder.Length == 0 || builder[^1] == ' '))
                return;
            builder.Append(str);
        }

        foreach (var emoteValue in emotes.Select(emote => emote is null ? " " : emote.ToString()!))
            AppendValue(emoteValue);

        return builder.ToString().Trim();
    }

    private List<IEmote?> FilterUnknownEmotes(List<IEmote?> emotes)
    {
        var guildEmotes = Context.Guild.Emotes;
        return emotes.FindAll(o => o is null or Emoji || guildEmotes.Any(x => Equals(x, o)));
    }
}
