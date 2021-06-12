using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Data;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Points
{
    [Group("points")]
    [Alias("body")]
    [Name("Body")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
    public class PointsModule : Infrastructure.ModuleBase
    {
        private PointsService PointsService { get; }
        private GrillBotContextFactory DbFactory { get; }

        public PointsModule(PointsService pointsService, GrillBotContextFactory dbFactory)
        {
            PointsService = pointsService;
            DbFactory = dbFactory;
        }

        [Command("where")]
        [Alias("kde", "gde")]
        [Summary("Získání aktuálního stavu bodů uživatele.")]
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
        public async Task GivePointsAsync([Name("mnozstvi")] int amount, [Name("uzivatel")] SocketUser user)
        {
            await PointsService.IncrementPointsAsync(Context.Guild, user, amount);
            await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}.");
        }

        [Command("transfer")]
        [Alias("preved")]
        [Summary("Převede určité množství bodů od jednoho uživatele druhému.")]
        public async Task TransferPointsAsync([Name("id/tag/jmeno_uzivatele (Od koho)")] SocketUser from, [Name("id/tag/jmeno_uzivatele (Komu)")] SocketUser to, [Name("mnozstvi")] int amount)
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
        public async Task GetPointsLeaderboardAsync()
        {
            using var dbContext = DbFactory.Create();

            var query = dbContext.GuildUsers.AsQueryable()
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
}
