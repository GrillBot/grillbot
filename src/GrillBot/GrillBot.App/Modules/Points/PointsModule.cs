using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Data.Exceptions;
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
    }
}
