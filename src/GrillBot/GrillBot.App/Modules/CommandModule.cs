using Discord.Commands;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    public class CommandModule : Infrastructure.ModuleBase
    {
        private IConfiguration Configuration { get; }

        public CommandModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("rerun")]
        [Summary("Znovu spustí příkaz z parametru.")]
        [RequireReference]
        public async Task<RuntimeResult> ReRunCommandAsync()
        {
            var prefix = Configuration.GetValue<string>("Discord:Commands:Prefix");

            if (!Context.Message.ReferencedMessage.Content.StartsWith(prefix))
            {
                await ReplyAsync("Zpráva v odpovědi není příkaz.");
                return null;
            }

            var newCommand = Context.Message.ReferencedMessage.Content[prefix.Length..];
            if (newCommand == "rerun")
            {
                await ReplyAsync("Tento příkaz nelze volat rekurzivně.");
                return null;
            }

            return new CommandRedirectResult(newCommand);
        }
    }
}
