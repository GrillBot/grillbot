using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased.User;

[RequireUserPerms(ContextType.Guild)]
public class UserMeModule : Infrastructure.ModuleBase
{
    [Command("me")]
    [TextCommandDeprecated(AlternativeCommand = "/me")]
    public Task GetMeInfoAsync() => Task.CompletedTask;
}
