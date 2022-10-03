using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.Common;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("points")]
[Alias("body")]
public class PointsModule : ModuleBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsModule(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

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
    [Summary("Získání TOP 10 statistik v počtu bodů.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(ContextType.Guild)]
    public async Task GetPointsLeaderboardAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var pointsBoard = await repository.Points.GetPointsBoardDataAsync(new[] { Context.Guild.Id.ToString() }, 10);
        if (pointsBoard.Count == 0)
        {
            await ReplyAsync("Ještě nebyly zachyceny žádné události ukazující aktivitu nějakého uživatele na serveru.");
            return;
        }

        var embed = new PointsBoardBuilder().WithBoard(Context.User, Context.Guild, pointsBoard, 0);

        var message = await ReplyAsync(embed: embed.Build());
        await message.AddReactionsAsync(Emojis.PaginationEmojis);
    }
}
