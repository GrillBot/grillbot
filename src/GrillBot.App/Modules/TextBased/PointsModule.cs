using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("points")]
[Alias("body")]
[ExcludeFromCodeCoverage]
public class PointsModule : ModuleBase
{
    [Command("where")]
    [Alias("kde", "gde")]
    [TextCommandDeprecated(AlternativeCommand = "/points where")]
    public Task GetPointsStateAsync(SocketUser user = null) => Task.CompletedTask;

    [Command("give")]
    [Alias("dej")]
    [TextCommandDeprecated(AdditionalMessage = "Servisní akce přidání a převodu bodů byly přesunuty do webové administrace.")]
    public Task GivePointsAsync(int amount, SocketGuildUser user) => Task.CompletedTask;

    [Command("transfer")]
    [Alias("preved")]
    [TextCommandDeprecated(AdditionalMessage = "Servisní akce přidání a převodu bodů byly přesunuty do webové administrace.")]
    public Task TransferPointsAsync(SocketGuildUser from, SocketGuildUser to, int amount) => Task.CompletedTask;

    [Command("board")]
    [TextCommandDeprecated(AlternativeCommand = "/points board")]
    public Task GetPointsLeaderboardAsync() => Task.CompletedTask;
}
