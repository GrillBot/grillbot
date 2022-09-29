using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Name("Náhodné věci")]
[ExcludeFromCodeCoverage]
public class MemeModule : ModuleBase
{
    [Command("peepolove")]
    [Alias("love")]
    [TextCommandDeprecated(AlternativeCommand = "/peepolove")]
    public Task PeepoloveAsync(IUser user = null) => Task.CompletedTask;

    [Command("peepoangry")]
    [Alias("angry")]
    [TextCommandDeprecated(AlternativeCommand = "/peepoangry")]
    public Task PeepoangryAsync(IUser user = null) => Task.CompletedTask;

    [Command("kachna")]
    [Alias("duck")]
    [TextCommandDeprecated(AlternativeCommand = "/kachna")]
    public Task GetDuckInfoAsync() => Task.CompletedTask;

    [Command("hi")]
    [Summary("Pozdraví uživatele")]
    [TextCommandDeprecated(AlternativeCommand = "/hi")]
    public Task HiAsync(int? _ = null) => Task.CompletedTask; // Command was reimplemented to Slash command.

    [Command("emojize")]
    [Summary("Znovu pošle zprávu jako emoji.")]
    [TextCommandDeprecated(AlternativeCommand = "/emojize")]
    public Task EmojizeAsync(string message = null) => Task.CompletedTask;

    [Command("reactjize")]
    [TextCommandDeprecated(AlternativeCommand = "/reactjize")]
    public Task ReactjizeAsync(string msg = null) => Task.CompletedTask;
}
