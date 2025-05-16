using Discord.Interactions;
using GrillBot.App.Actions.Commands.Emotes.Suggestions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions.Emote;

[Group("emote-suggestions-admin", "Emote suggestions (Administration)")]
[RequireUserPerms]
public class SuggestionAdminModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [SlashCommand("start-vote", "Starts vote for approved suggestions.")]
    public async Task StartVoteAsync()
    {
        using var command = await GetCommandAsync<StartVoteAction>();
        var message = await command.Command.ProcessAsync();

        await SetResponseAsync(message);
    }
}
