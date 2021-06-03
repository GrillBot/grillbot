using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Data.Exceptions;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Points
{
    [Group("points")]
    [Alias("body")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
    public class PointsModule : Infrastructure.ModuleBase
    {
        private PointsService PointsService { get; }

        public PointsModule(PointsService pointsService)
        {
            PointsService = pointsService;
        }

        [Command("where")]
        [Alias("kde", "gde")]
        [Summary("Získání aktuálního stavu bodů uživatele.")]
        public async Task GetPointsStateAsync(SocketUser user = null)
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
        public async Task GivePointsAsync(int amount, SocketUser user)
        {
            await PointsService.IncrementPointsAsync(Context.Guild, user, amount);
            await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}.");
        }

        [Command("transfer")]
        [Alias("preved")]
        [Summary("Převede určité množství bodů od jednoho uživatele druhému.")]
        public async Task TransferPointsAsync(SocketUser from, SocketUser to, int amount)
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
    }
}
