using Discord.Interactions;
using GrillBot.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var service = services.GetRequiredService<SearchingService>();
        var suggestions = await service.GenerateSuggestionsAsync(context.User as IGuildUser, context.Guild, context.Channel);

        var result = suggestions.Select(o => new AutocompleteResult(o.Value, o.Key));
        return AutocompletionResult.FromSuccess(result);
    }
}
