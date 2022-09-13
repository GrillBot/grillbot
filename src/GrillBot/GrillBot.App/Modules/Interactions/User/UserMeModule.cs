using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.User;

namespace GrillBot.App.Modules.Interactions.User;

[RequireUserPerms]
public class UserMeModule : InteractionsModuleBase
{
    private UserService UserService { get; }

    public UserMeModule(UserService userService)
    {
        UserService = userService;
    }

    [SlashCommand("me", "Information about me")]
    public async Task GetInfoAboutMeAsync()
    {
        var user = Context.User as SocketGuildUser ?? Context.Guild.GetUser(Context.User.Id);
        var embed = await UserService.CreateInfoEmbed(Context.User, Context.Guild, user);

        await SetResponseAsync(embed: embed);
    }
}
