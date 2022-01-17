using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Database.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace GrillBot.Data.Modules.TextBased.User;

[Name("Správa uživatelů")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class UserMeModule : Infrastructure.ModuleBase
{
    private GrillBotContextFactory DbFactory { get; }
    private IConfiguration Configuration { get; }

    public UserMeModule(GrillBotContextFactory dbFactory, IConfiguration configuration)
    {
        Configuration = configuration;
        DbFactory = dbFactory;
    }

    [Command("me")]
    [Summary("Informace o uživateli, který volá příkaz.")]
    public async Task GetMeInfoAsync()
    {
        var user = Context.User is SocketGuildUser guildUser ? guildUser : Context.Guild.GetUser(Context.User.Id);
        var embed = await UserModule.GetUserInfoEmbedAsync(Context, DbFactory, Configuration, user);

        await ReplyAsync(embed: embed);
    }
}
