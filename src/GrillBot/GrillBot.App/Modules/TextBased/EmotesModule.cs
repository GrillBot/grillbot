using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;

namespace GrillBot.App.Modules.TextBased;

[Group("emote")]
public class EmotesModule : Infrastructure.ModuleBase
{
    [Group("list")]
    public class EmoteListSubModule : Infrastructure.ModuleBase
    {
        [Command]
        [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
        public Task GetListByCount(IUser _ = null) => Task.CompletedTask;

        [Group("count")]
        public class EmoteListByCountSubModule : Infrastructure.ModuleBase
        {
            [Command("desc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetDescendingListByCount(IUser _ = null) => Task.CompletedTask;

            [Command("asc")]
            [TextCommandDeprecated(AlternativeCommand = "/emote", AdditionalMessage = "Všechny příkazy pro práci s emote statistikou byly přesunuty pod lomítko")]
            public Task GetAscendingListByCount(IUser _ = null) => Task.CompletedTask;
        }

        [Group("lastuse")]
        public class EmoteListByLastUseSubModule : Infrastructure.ModuleBase
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
