using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.User;

[ExcludeFromCodeCoverage]
public class UserMeModule : ModuleBase
{
    [Command("me")]
    [TextCommandDeprecated(AlternativeCommand = "/me")]
    public Task GetMeInfoAsync() => Task.CompletedTask;
}
