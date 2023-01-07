using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetSuggestions : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public GetSuggestions(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestions = await repository.Remind.GetRemindSuggestionsAsync(Context.User);
        var userId = Context.User.Id.ToString();

        var incoming = suggestions
            .Where(o => o.ToUserId == userId)
            .Select(o => new AutocompleteResult(GetRow(true, o), o.Id));

        var outgoing = suggestions
            .Where(o => o.FromUserId == userId)
            .Select(o => new AutocompleteResult(GetRow(false, o), o.Id));

        return incoming.Concat(outgoing)
            .DistinctBy(o => o.Value)
            .OrderBy(o => o.Value)
            .Take(25)
            .ToList();
    }

    private string GetRow(bool incoming, RemindMessage remind)
    {
        var textId = incoming ? "Incoming" : "Outgoing";
        return Texts[$"RemindModule/Suggestions/{textId}", Locale].FormatWith(remind.Id, remind.At.ToCzechFormat(), remind.FromUser!.FullName());
    }
}
