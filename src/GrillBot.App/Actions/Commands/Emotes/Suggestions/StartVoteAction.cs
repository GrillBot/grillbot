using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using Emote;

namespace GrillBot.App.Actions.Commands.Emotes.Suggestions;

public class StartVoteAction(
    IServiceClientExecutor<IEmoteServiceClient> _emoteService,
    ITextsManager _texts
) : CommandAction
{
    public async Task<string> ProcessAsync()
    {
        try
        {
            var startedVotesCount = await _emoteService.ExecuteRequestAsync(
                (client, ctx) => client.StartSuggestionsVotingAsync(Context.Guild.Id, ctx.CancellationToken)
            );

            return startedVotesCount == 0
                ? _texts["SuggestionModule/NoVote", Locale]
                : _texts["SuggestionModule/VoteStarted", Locale].FormatWith(startedVotesCount);
        }
        catch (ClientBadRequestException ex)
        {
            var messages = ex.ValidationErrors
                .SelectMany(o => o.Value)
                .Distinct();

            return string.Join("\n", messages);
        }
    }
}
