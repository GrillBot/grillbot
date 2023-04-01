using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions.User;

[RequireUserPerms]
public class UserMeModule : InteractionsModuleBase
{
    public UserMeModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("me", "Information about me")]
    public async Task GetInfoAboutMeAsync()
    {
        var user = Context.User as IGuildUser ?? Context.Guild.GetUser(Context.User.Id);

        using var command = GetCommand<Actions.Commands.UserInfo>();
        var result = await command.Command.ProcessAsync(user);
        await SetResponseAsync(embed: result);
    }
}
