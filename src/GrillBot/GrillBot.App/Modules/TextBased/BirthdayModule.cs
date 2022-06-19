using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("birthday")]
[Alias("narozeniny")]
public class BirthdayModule : ModuleBase
{
    [Command("", ignoreExtraArgs: true)]
    [TextCommandDeprecated(AlternativeCommand = "/narozeniny", AdditionalMessage = "Všechny příkazy pro práci s narozeninami byly přesunuty jako příkazy s lomítkem.")]
    public Task TodayBrithdayAsync() => Task.CompletedTask;
}
