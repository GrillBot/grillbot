using Discord.Interactions;
using GrillBot.App.Services.Reminder;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        var service = services.GetRequiredService<RemindService>();
        var suggestions = await service.GetRemindSuggestionsAsync(context.User);

        var result = suggestions.Select(o => new AutocompleteResult(o.Value, o.Key));
        return AutocompletionResult.FromSuccess(result);
    }
}
