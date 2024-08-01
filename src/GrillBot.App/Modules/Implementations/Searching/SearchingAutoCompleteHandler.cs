using Discord.Interactions;
using GrillBot.App.Actions.Commands.Searching;
using GrillBot.App.Infrastructure;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingAutoCompleteHandler : BaseAutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter,
        IServiceProvider services)
    {
        using var command = await CreateCommandAsync<GetSuggestions>(context, services);

        command.Command.Init(context);
        var result = await command.Command.ProcessAsync();
        return AutocompletionResult.FromSuccess(result);
    }
}
