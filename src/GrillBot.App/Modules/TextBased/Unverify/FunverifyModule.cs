using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.Unverify;

public class FunverifyModule : ModuleBase
{
    [Command("funverify")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify fun")]
    public Task FunverifyAsync(DateTime end, string data) => Task.CompletedTask;
}
