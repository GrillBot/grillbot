using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.Unverify;

[Group("unverify")]
public class UnverifyModule : ModuleBase
{

    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify set")]
    public Task SetUnverifyAsync(DateTime end, string data) => Task.CompletedTask;

    [Command("remove")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify remove")]
    public Task RemoveUnverifyAsync(IGuildUser user) => Task.CompletedTask;

    [Command("update")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify update")]
    public Task UnverifyUpdateAsync(IGuildUser user, DateTime end) => Task.CompletedTask;

    [Command("list")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify list")]
    public Task UnverifyListAsync() => Task.CompletedTask;
}
