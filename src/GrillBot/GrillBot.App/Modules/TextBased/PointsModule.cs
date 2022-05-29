﻿using Discord.Commands;
using GrillBot.App.Modules.Implementations.Points;
using GrillBot.App.Services.User;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.TextBased;

[Group("points")]
[Alias("body")]
[Name("Body")]
public class PointsModule : Infrastructure.ModuleBase
{
    private PointsService PointsService { get; }
    private GrillBotDatabaseFactory DbFactory { get; }

    public PointsModule(PointsService pointsService, GrillBotDatabaseFactory dbFactory)
    {
        PointsService = pointsService;
        DbFactory = dbFactory;
    }

    [Command("where")]
    [Alias("kde", "gde")]
    [Summary("Získání aktuálního stavu bodů uživatele.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(ContextType.Guild)]
    public async Task GetPointsStateAsync([Name("id/tag/jmeno_uzivatele")] SocketUser user = null)
    {
        if (user == null) user = Context.User;

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
        await PointsService.IncrementPointsAsync(Context.Guild, user, amount);
        await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}.");
    }

    [Command("transfer")]
    [Alias("preved")]
    [Summary("Převede určité množství bodů od jednoho uživatele druhému.")]
    [Infrastructure.Preconditions.TextBased.RequireUserPerms(GuildPermission.Administrator)]
    public async Task TransferPointsAsync([Name("id/tag/jmeno_uzivatele (Od koho)")] SocketUser from, [Name("id/tag/jmeno_uzivatele (Komu)")] SocketGuildUser to, [Name("mnozstvi")] int amount)
    {
        try
        {
            await PointsService.TransferPointsAsync(Context.Guild, from, to, amount);
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
        using var dbContext = DbFactory.Create();

        var query = dbContext.GuildUsers.AsNoTracking()
            .Where(o => o.GuildId == Context.Guild.Id.ToString() && o.Points > 0)
            .OrderByDescending(o => o.Points)
            .Select(o => new KeyValuePair<string, long>(o.UserId, o.Points))
            .Take(10);

        if (!await query.AnyAsync())
        {
            await ReplyAsync("Ještě nebyly zachyceny žádné události ukazující aktivitu nějakého uživatele na serveru.");
            return;
        }

        var data = await query.ToListAsync();

        await Context.Guild.DownloadUsersAsync();
        var embed = new PointsBoardBuilder()
            .WithBoard(Context.User, Context.Guild, data, id => Context.Guild.GetUser(id), 0);

        var message = await ReplyAsync(embed: embed.Build());
        await message.AddReactionsAsync(Emojis.PaginationEmojis);
    }
}
