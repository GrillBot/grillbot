using Discord.Commands;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.App.Services.User.Points;
using GrillBot.Common;
using GrillBot.Data.Exceptions;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("points")]
[Alias("body")]
[Name("Body")]
public class PointsModule : ModuleBase
{
    private PointsService PointsService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsModule(PointsService pointsService, GrillBotDatabaseBuilder databaseBuilder)
    {
        PointsService = pointsService;
        DatabaseBuilder = databaseBuilder;
    }

    [Command("where")]
    [Alias("kde", "gde")]
    [Summary("Získání aktuálního stavu bodů uživatele.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(ContextType.Guild)]
    public async Task GetPointsStateAsync([Name("id/tag/jmeno_uzivatele")] SocketUser user = null)
    {
        user ??= Context.User;

        try
        {
            using var img = await PointsService.GetPointsOfUserImageAsync(Context.Guild, user);
            await ReplyFileAsync(img.Path, false);
        }
        catch (NotFoundException ex)
        {
            await ReplyAsync(ex.Message);
        }
    }

    [Command("give")]
    [Alias("dej")]
    [Summary("Přidá uživateli zadané množství bodů.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(GuildPermission.Administrator)]
    public async Task GivePointsAsync([Name("mnozstvi")] int amount, [Name("uzivatel")] SocketGuildUser user)
    {
        await PointsService.IncrementPointsAsync(user, amount);
        await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}.");
    }

    [Command("transfer")]
    [Alias("preved")]
    [Summary("Převede určité množství bodů od jednoho uživatele druhému.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(GuildPermission.Administrator)]
    public async Task TransferPointsAsync([Name("id/tag/jmeno_uzivatele (Od koho)")] SocketGuildUser from, [Name("id/tag/jmeno_uzivatele (Komu)")] SocketGuildUser to, [Name("mnozstvi")] int amount)
    {
        try
        {
            await PointsService.TransferPointsAsync(from, to, amount);
            await ReplyAsync("Body byly úspěšně převedeny.");
        }
        catch (InvalidOperationException ex)
        {
            await ReplyAsync(ex.Message);
        }
    }

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
