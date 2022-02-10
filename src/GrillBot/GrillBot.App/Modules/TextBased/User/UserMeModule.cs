using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased.User;

[Name("Správa uživatelů")]
[RequireUserPerms(ContextType.Guild)]
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
