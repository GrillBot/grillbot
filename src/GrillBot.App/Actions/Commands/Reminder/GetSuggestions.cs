using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Response;

namespace GrillBot.App.Actions.Commands.Reminder;

public class GetSuggestions(
    ITextsManager _texts,
    IServiceClientExecutor<IRemindServiceClient> _remindServiceClient,
    DataResolveManager _dataResolve
) : CommandAction
{
    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        var suggestions = await _remindServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetSuggestionsAsync(Context.User.Id.ToString(), cancellationToken));
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
