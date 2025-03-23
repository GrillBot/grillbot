using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class MathModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [SlashCommand("solve", "Calculates a mathematical expression.")]
    public async Task SolveExpressionAsync(
        [Summary("expression", "Mathematical expression to calculate.")]
        string expression
    )
    {
        using var command = GetCommand<Actions.Commands.SolveExpression>();

        var result = await command.Command.ProcessAsync(expression);
        await SetResponseAsync(embed: result);
    }
}
