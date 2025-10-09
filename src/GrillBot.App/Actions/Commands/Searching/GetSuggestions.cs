using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Executor;
using SearchingService;

namespace GrillBot.App.Actions.Commands.Searching;

public class GetSuggestions(
    IServiceClientExecutor<ISearchingServiceClient> _searchingService,
    DataResolveManager _dataResolve
) : CommandAction
{
    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        var guildId = Context.Guild.Id.ToString();
        var channelId = Context.Channel.Id.ToString();
        var suggestions = await _searchingService.ExecuteRequestAsync((c, ctx) => c.GetSuggestionsAsync(guildId, channelId, ctx.AuthorizationToken, ctx.CancellationToken));
        var result = new List<AutocompleteResult>();

        foreach (var item in suggestions)
        {
            var user = await _dataResolve.GetUserAsync(item.UserId.ToUlong());
            result.Add(new($"#{item.Id} - {user!.Username} ({item.ShortenMessage})", item.Id));
        }

        return result;
    }
}
