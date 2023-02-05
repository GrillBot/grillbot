using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Infrastructure.Commands;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[ExcludeFromCodeCoverage]
public class MathModule : InteractionsModuleBase
{
    public MathModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

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
