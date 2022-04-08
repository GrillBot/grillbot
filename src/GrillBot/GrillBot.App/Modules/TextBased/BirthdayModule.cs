using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

[Group("birthday")]
[Alias("narozeniny")]
public class BirthdayModule : Infrastructure.ModuleBase
{
    [Command("", ignoreExtraArgs: true)]
    [TextCommandDeprecated(AlternativeCommand = "/narozeniny", AdditionalMessage = "Všechny příkazy pro práci s narozeninami byly přesunuty jako příkazy s lomítkem.")]
    public Task TodayBrithdayAsync() => Task.CompletedTask;
}
