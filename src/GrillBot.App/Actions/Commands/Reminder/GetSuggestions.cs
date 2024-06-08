using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Response;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetSuggestions : CommandAction
{
    private readonly IRemindServiceClient _remindServiceClient;
    private readonly DataResolveManager _dataResolve;
    private readonly ITextsManager _texts;

    public GetSuggestions(ITextsManager texts, IRemindServiceClient remindServiceClient, DataResolveManager dataResolve)
    {
        _remindServiceClient = remindServiceClient;
        _dataResolve = dataResolve;
        _texts = texts;
    }

    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        var suggestions = await _remindServiceClient.GetSuggestionsAsync(Context.User.Id.ToString());
        var result = new List<AutocompleteResult>();

        foreach (var item in suggestions.Take(25))
        {
            var row = await GetRowAsync(item);
            result.Add(new AutocompleteResult(row, item.RemindId));
        }

        return result;
    }

    private async Task<string> GetRowAsync(ReminderSuggestionItem item)
    {
        var textId = item.IsIncoming ? "Incoming" : "Outgoing";
        var messageTemplate = _texts[$"RemindModule/Suggestions/{textId}", Locale];
        var at = item.NotifyAtUtc.ToLocalTime().ToCzechFormat();
        var fromUser = await _dataResolve.GetUserAsync(item.FromUserId.ToUlong());

        return messageTemplate.FormatWith(item.RemindId, at, fromUser!.Username);
    }
}
