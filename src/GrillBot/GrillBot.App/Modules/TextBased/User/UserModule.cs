using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.User;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.User;

[Group("user")]
[Name("Správa uživatelů")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class UserModule : ModuleBase
{
    private UserService UserService { get; }

    public const int UserAccessMaxCountPerPage = 15;

    public UserModule(UserService userService)
    {
        UserService = userService;
    }

    [Command("info")]
    [Summary("Získání informací o uživateli.")]
    [RequireUserPerms(GuildPermission.ViewAuditLog)]
    public async Task GetUserInfoAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
    {
        user ??= Context.User;
        if (user is not SocketGuildUser guildUser) return;

        var embed = await UserService.CreateInfoEmbed(Context.User, Context.Guild, guildUser);
        await ReplyAsync(embed: embed);
    }

    [Command("access")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění v kanálech.")]
    [RequireUserPerms(GuildPermission.ManageRoles)]
    [TextCommandDeprecated(AlternativeCommand = "/user access", AdditionalMessage = "Případně lze příkaz zavolat i z kontextové nabídky uživatele.")]
    public Task GetUsersAccessListAsync(params IGuildUser[] _) => Task.CompletedTask;
}
