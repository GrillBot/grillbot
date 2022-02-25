using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

public class MathModule : Infrastructure.ModuleBase
{
    [Command("solve")]
    [TextCommandDeprecated(AlternativeCommand = "/solve")]
    public Task SolveExpressionAsync([Remainder][Name("vyraz")] string _) => Task.CompletedTask;
}
