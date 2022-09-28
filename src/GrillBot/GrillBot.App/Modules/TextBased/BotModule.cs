using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[ExcludeFromCodeCoverage]
public class BotModule : ModuleBase
{
    [Command("bot")]
    [Alias("about", "o")]
    [TextCommandDeprecated(AlternativeCommand = "/bot info")]
    public Task BotInfoAsync() => Task.CompletedTask;
}
