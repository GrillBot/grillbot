using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.User;

namespace GrillBot.App.Modules.TextBased.User;

[Name("Správa uživatelů")]
[RequireUserPerms(ContextType.Guild)]
public class UserMeModule : Infrastructure.ModuleBase
{
    private UserService UserService { get; }

    public UserMeModule(UserService userService)
    {
        UserService = userService;
    }

    [Command("me")]
    [Summary("Informace o uživateli, který volá příkaz.")]
    public async Task GetMeInfoAsync()
    {
        var user = Context.User is SocketGuildUser guildUser ? guildUser : Context.Guild.GetUser(Context.User.Id);
        var embed = await UserService.CreateInfoEmbed(Context, user);

        await ReplyAsync(embed: embed);
    }
}
