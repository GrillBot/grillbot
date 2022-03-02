using Discord.Commands;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.App.Services.Emotes;

namespace GrillBot.App.Modules.TextBased;

[Group("emote")]
[Name("Emotes")]
[Summary("Správa emotů")]
[RequireUserPerms(ContextType.Guild)]
public class EmotesModule : Infrastructure.ModuleBase
{
    [Group("list")]
    [Name("Seznam emotů")]
    [Summary("Získání seznamu statistiky emotů")]
    public class EmoteListSubModule : Infrastructure.ModuleBase
    {
        [Command]
        [Summary("Získání seznamu statistiky emotů podle počtu použití.")]
        public Task<RuntimeResult> GetListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null) => Task.FromResult(new CommandRedirectResult($"emote list count desc {user?.Mention}".Trim()) as RuntimeResult);

        [Group("count")]
        [Name("Seznam emotů")]
        [Summary("Získání seznamu statistiky emotů podle počtu použití.")]
        public class EmoteListByCountSubModule : Infrastructure.ModuleBase
        {
            private EmoteService EmoteService { get; }

            public EmoteListByCountSubModule(EmoteService emoteService)
            {
                EmoteService = emoteService;
            }

            [Command("desc")]
            [Summary("Získání seznamu statistiky emotů podle počtu použití sestupně.")]
            public async Task GetDescendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
            {
                var embed = await CreateEmoteListEmbedAsync(EmoteService, Context, user, "count/desc", 0);
                await ReplyPaginatedAsync(embed);
            }

            [Command("asc")]
            [Summary("Získání seznamu statistiky emotů podle počtu použití vzestupně.")]
            public async Task GetAscendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
            {
                var embed = await CreateEmoteListEmbedAsync(EmoteService, Context, user, "count/asc", 0);
                await ReplyPaginatedAsync(embed);
            }
        }

        [Group("lastuse")]
        [Name("Seznam emotů")]
        [Summary("Získání seznamu statistiky emotů podle data posledního použití.")]
        public class EmoteListByLastUseSubModule : Infrastructure.ModuleBase
        {
            private EmoteService EmoteService { get; }

            public EmoteListByLastUseSubModule(EmoteService emoteService)
            {
                EmoteService = emoteService;
            }

            [Command("desc")]
            [Summary("Získání seznamu statistiky emotů podle data posledního použití sestupně.")]
            public async Task GetDescendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
            {
                var embed = await CreateEmoteListEmbedAsync(EmoteService, Context, user, "lastuse/desc", 0);
                await ReplyPaginatedAsync(embed);
            }

            [Command("asc")]
            [Summary("Získání seznamu statistiky emotů podle data posledního použití vzestupně.")]
            public async Task GetAscendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
            {
                var embed = await CreateEmoteListEmbedAsync(EmoteService, Context, user, "lastuse/asc", 0);
                await ReplyPaginatedAsync(embed);
            }
        }

        public static async Task<Embed> CreateEmoteListEmbedAsync(EmoteService emoteService, SocketCommandContext context, IUser ofUser, string sortQuery, int page)
        {
            var data = await emoteService.GetEmoteListAsync(context.Guild, ofUser, sortQuery);

            var list = new EmbedBuilder().WithEmoteList(data, context.User, ofUser, context.Guild, sortQuery, page);
            return list.Build();
        }
    }

    [Command("get")]
    [Summary("Získá informace o požadovaném emote.")]
    [TextCommandDeprecated(AlternativeCommand = "/emote get")]
    public Task GetEmoteInfoAsync([Name("emote/id/nazev emote")] IEmote _) => Task.CompletedTask;
}
