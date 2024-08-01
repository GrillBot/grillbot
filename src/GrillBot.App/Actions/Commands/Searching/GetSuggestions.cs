using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.SearchingService;

namespace GrillBot.App.Actions.Commands.Searching;

public class GetSuggestions : CommandAction
{
    private readonly ISearchingServiceClient _searchingService;
    private readonly DataResolveManager _dataResolve;

    public GetSuggestions(ISearchingServiceClient searchingService, DataResolveManager dataResolve)
    {
        _searchingService = searchingService;
        _dataResolve = dataResolve;
    }

    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        var guildId = Context.Guild.Id.ToString();
        var channelId = Context.Channel.Id.ToString();
        var suggestions = await _searchingService.GetSuggestionsAsync(guildId, channelId);
        var result = new List<AutocompleteResult>();

        foreach (var item in suggestions)
        {
            var user = await _dataResolve.GetUserAsync(item.UserId.ToUlong());
            result.Add(new($"#{item.Id} - {user!.Username} ({item.ShortenMessage})", item.Id));
        }

        return result;
    }
}
