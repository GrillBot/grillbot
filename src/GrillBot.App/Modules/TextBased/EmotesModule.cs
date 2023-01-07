using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Group("emote")]
[ExcludeFromCodeCoverage]
public class EmotesModule : ModuleBase
{
    [Group("list")]
    public class EmoteListSubModule : ModuleBase
    {
        [Command]
        [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
        public Task GetListByCount(IUser _ = null) => Task.CompletedTask;

        [Group("count")]
        public class EmoteListByCountSubModule : ModuleBase
        {
            [Command("desc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetDescendingListByCount(IUser _ = null) => Task.CompletedTask;

            [Command("asc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetAscendingListByCount(IUser _ = null) => Task.CompletedTask;
        }

        [Group("lastuse")]
        public class EmoteListByLastUseSubModule : ModuleBase
        {
            [Command("desc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetDescendingListByCount(IUser _ = null) => Task.CompletedTask;

            [Command("asc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetAscendingListByCount(IUser _ = null) => Task.CompletedTask;
        }
    }

    [Command("get")]
    [TextCommandDeprecated(AlternativeCommand = "/emote get")]
    public Task GetEmoteInfoAsync([Name("emote/id/nazev emote")] IEmote _) => Task.CompletedTask;
}
