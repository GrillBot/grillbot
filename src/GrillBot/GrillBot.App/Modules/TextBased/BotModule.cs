using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

public class BotModule : Infrastructure.ModuleBase
{
    [Command("bot")]
    [Alias("about", "o")]
    [TextCommandDeprecated(AlternativeCommand = "/about")]
    public Task BotInfoAsync() => Task.CompletedTask;
}
