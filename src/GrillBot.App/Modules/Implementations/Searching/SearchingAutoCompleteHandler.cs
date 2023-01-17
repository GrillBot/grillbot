using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter,
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.Searching.GetSuggestions>();
        action.Init(context);

        var result = await action.ProcessAsync();
        return AutocompletionResult.FromSuccess(result);
    }
}
