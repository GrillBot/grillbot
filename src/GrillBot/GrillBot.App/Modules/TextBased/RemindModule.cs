using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

[Group("remind")]
public class RemindModule : Infrastructure.ModuleBase
{
    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/remind", AdditionalMessage = "Všechny příkazy pro práci s připomínáním byly přesunuty jako příkazy s lomítkem.")]
    public Task GetRemindListAsync() => Task.CompletedTask;
}
