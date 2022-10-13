using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.User;

[Group("user")]
[ExcludeFromCodeCoverage]
public class UserModule : ModuleBase
{
    [Command("info")]
    [TextCommandDeprecated(AlternativeCommand = "/user info")]
    public Task GetUserInfoAsync(IUser user = null) => Task.CompletedTask;

    [Command("access")]
    [TextCommandDeprecated(AlternativeCommand = "/user access", AdditionalMessage = "Případně lze příkaz zavolat i z kontextové nabídky uživatele.")]
    public Task GetUsersAccessListAsync(params IGuildUser[] _) => Task.CompletedTask;
}
