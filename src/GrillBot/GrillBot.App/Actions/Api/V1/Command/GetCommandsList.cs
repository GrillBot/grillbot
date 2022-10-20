using Discord.Interactions;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetCommandsList : ApiAction
{
    private InteractionService InteractionService { get; }

    public GetCommandsList(ApiRequestContext apiContext, InteractionService interactionService) : base(apiContext)
    {
        InteractionService = interactionService;
    }

    public List<string> Process()
    {
        return InteractionService.SlashCommands
            .Select(o => o.ToString().Trim())
            .Where(o => !string.IsNullOrEmpty(o))
            .Select(o => $"/{o}")
            .Distinct()
            .OrderBy(o => o[1..])
            .ToList();
    }
}
