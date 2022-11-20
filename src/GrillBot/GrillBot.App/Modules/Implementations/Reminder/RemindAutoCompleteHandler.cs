using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Actions.Commands.Reminder;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Reminder;

[ExcludeFromCodeCoverage]
public class RemindAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter,
        IServiceProvider services)
    {
        using var scope = services.CreateScope();
        
        var getSuggestions = scope.ServiceProvider.GetRequiredService<GetSuggestions>();
        getSuggestions.Init(context);

        var result = await getSuggestions.ProcessAsync();
        return AutocompletionResult.FromSuccess(result);
    }
}
