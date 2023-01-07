using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("channel")]
[ExcludeFromCodeCoverage]
public class ChannelModule : ModuleBase
{
    [Command("board")]
    [TextCommandDeprecated(AlternativeCommand = "/channel board")]
    public Task GetChannelBoardAsync() => Task.CompletedTask;
}
