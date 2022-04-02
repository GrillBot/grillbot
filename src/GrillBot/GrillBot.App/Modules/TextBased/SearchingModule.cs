using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

[Group("hledam")]
public class SearchingModule : Infrastructure.ModuleBase
{
    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/hledam", AdditionalMessage = "Všechny příkazy pro práci s hledáním byly přesunuty jako příkazy s lomítkem.")]
    public Task CreateSearchAsync([Remainder][Name("obsah")] string _) => Task.CompletedTask;
}
