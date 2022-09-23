using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetCommandsList : ApiAction
{
    private CommandService CommandService { get; }
    private InteractionService InteractionService { get; }
    private IConfiguration Configuration { get; }

    public GetCommandsList(ApiRequestContext apiContext, CommandService commandService, InteractionService interactionService, IConfiguration configuration) : base(apiContext)
    {
        CommandService = commandService;
        InteractionService = interactionService;
        Configuration = configuration;
    }

    public List<string> Process()
    {
        var commands = CommandService.Modules
            .Where(o => o.Commands.Count > 0 && !o.Preconditions.OfType<TextCommandDeprecatedAttribute>().Any())
            .Select(o => o.Commands.Where(x => !x.Preconditions.OfType<TextCommandDeprecatedAttribute>().Any()))
            .SelectMany(o => o.Select(x => Configuration.GetValue<string>("Discord:Commands:Prefix") + x.Aliases[0].Trim()).Distinct())
            .Distinct();

        var slashCommands = InteractionService.SlashCommands
            .Select(o => o.ToString().Trim())
            .Where(o => !string.IsNullOrEmpty(o))
            .Select(o => $"/{o}")
            .Distinct();

        return commands.Concat(slashCommands).OrderBy(o => o[1..]).ToList();
    }
}
