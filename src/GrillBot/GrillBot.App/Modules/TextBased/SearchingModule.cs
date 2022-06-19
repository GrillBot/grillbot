using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("hledam")]
public class SearchingModule : ModuleBase
{
    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/hledam", AdditionalMessage = "Všechny příkazy pro práci s hledáním byly přesunuty jako příkazy s lomítkem.")]
    public Task CreateSearchAsync([Remainder][Name("obsah")] string _) => Task.CompletedTask;
}
