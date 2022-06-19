using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

public class MathModule : ModuleBase
{
    [Command("solve")]
    [TextCommandDeprecated(AlternativeCommand = "/solve")]
    public Task SolveExpressionAsync([Remainder] [Name("vyraz")] string _) => Task.CompletedTask;
}
