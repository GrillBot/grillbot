#pragma warning disable IDE0060 // Remove unused parameter
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.Unverify;

[Group("selfunverify")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class SelfUnverifyModule : ModuleBase
{
    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/selfunverify")]
    public Task SelfunverifyAsync([Name("datum konce")] DateTime end, [Name("seznam ponechatelnych")] params string[] keeps) => Task.CompletedTask;
}
