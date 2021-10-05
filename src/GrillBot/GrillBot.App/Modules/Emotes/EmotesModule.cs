using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Data;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Emotes
{
    [Group("emote")]
    [Name("Emotes")]
    [Summary("Správa emotů")]
    public class EmotesModule : Infrastructure.ModuleBase
    {
        private GrillBotContextFactory DbFactory { get; }

        public EmotesModule(GrillBotContextFactory dbFactory)
        {
            DbFactory = dbFactory;
        }

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
                private GrillBotContextFactory DbFactory { get; }

                public EmoteListByCountSubModule(GrillBotContextFactory dbFactory)
                {
                    DbFactory = dbFactory;
                }

                [Command("desc")]
                [Summary("Získání seznamu statistiky emotů podle počtu použití sestupně.")]
                public Task GetDescendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
                {
                    return CreateAndSendEmoteList(DbFactory, Context, user, "count", true, 0,
                        o => o.OrderByDescending(x => x.Sum(t => t.UseCount)).ThenByDescending(o => o.Max(x => x.LastOccurence)),
                        embed => ReplyAsync(embed: embed)
                    );
                }

                [Command("asc")]
                [Summary("Získání seznamu statistiky emotů podle počtu použití vzestupně.")]
                public Task GetAscendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
                {
                    return CreateAndSendEmoteList(DbFactory, Context, user, "count", false, 0,
                        o => o.OrderBy(x => x.Sum(t => t.UseCount)).ThenBy(o => o.Max(x => x.LastOccurence)),
                        embed => ReplyAsync(embed: embed)
                    );
                }
            }

            [Group("lastuse")]
            [Name("Seznam emotů")]
            [Summary("Získání seznamu statistiky emotů podle data posledního použití.")]
            public class EmoteListByLastUseSubModule : Infrastructure.ModuleBase
            {
                private GrillBotContextFactory DbFactory { get; }

                public EmoteListByLastUseSubModule(GrillBotContextFactory dbFactory)
                {
                    DbFactory = dbFactory;
                }

                [Command("desc")]
                [Summary("Získání seznamu statistiky emotů podle data posledního použití sestupně.")]
                public Task GetDescendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
                {
                    return CreateAndSendEmoteList(DbFactory, Context, user, "lastuse", true, 0,
                        o => o.OrderByDescending(x => x.Max(t => t.LastOccurence)).ThenByDescending(x => x.Sum(t => t.UseCount)),
                        embed => ReplyAsync(embed: embed)
                    );
                }

                [Command("asc")]
                [Summary("Získání seznamu statistiky emotů podle data posledního použití vzestupně.")]
                public Task GetAscendingListByCount([Name("id/tag/jmeno_uzivatele")] IUser user = null)
                {
                    return CreateAndSendEmoteList(DbFactory, Context, user, "lastuse", false, 0,
                        o => o.OrderBy(x => x.Max(t => t.LastOccurence)).ThenBy(x => x.Sum(t => t.UseCount)),
                        embed => ReplyAsync(embed: embed)
                    );
                }
            }

            public static async Task CreateAndSendEmoteList(GrillBotContextFactory factory, SocketCommandContext context, IUser user,
                string type, bool sortDesc, int page,
                Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> orderFunc,
                Func<Embed, Task<IUserMessage>> replyFunc)
            {
                using var dbContext = factory.Create();

                var query = GetListQuery(dbContext, user?.Id, orderFunc, null, EmbedBuilder.MaxFieldCount);
                var data = await query.ToListAsync();

                var list = new EmbedBuilder().WithEmoteList(data, context.User, user, context.IsPrivate, sortDesc, type, page);
                var message = await replyFunc(list.Build());

                if (data.Count > 0)
                    await message.AddReactionsAsync(Emojis.PaginationEmojis);
            }

            public static IQueryable<Tuple<string, int, long, DateTime, DateTime>> GetListQuery(GrillBotContext context, ulong? userId,
                Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> orderFunc,
                int? skip, int? take)
            {
                var query = context.Emotes.AsQueryable().AsNoTracking();

                if (userId != null)
                    query = query.Where(o => o.UserId == userId.ToString());

                var groupQuery = query.GroupBy(o => o.EmoteId);
                groupQuery = orderFunc(groupQuery);

                var resultQuery = groupQuery.Select(o => new Tuple<string, int, long, DateTime, DateTime>(
                    o.Key,
                    o.Count(),
                    o.Sum(x => x.UseCount),
                    o.Min(x => x.FirstOccurence),
                    o.Max(x => x.LastOccurence)
                ));

                if (skip != null)
                    resultQuery = resultQuery.Skip(skip.Value);

                if (take != null)
                    resultQuery = resultQuery.Take(take.Value);

                return resultQuery;
            }
        }

        [Command("get")]
        [Summary("Získá informace o požadovaném emote.")]
        public async Task GetEmoteInfoAsync([Name("samotny/id/nazev emote")] IEmote emote)
        {
            if (emote is not Emote _emote)
            {
                await ReplyAsync("Unicode emoji nejsou v tomto příkazu podporovány.");
                return;
            }

            using var dbContext = DbFactory.Create();
            var baseQuery = dbContext.Emotes.AsQueryable().Where(o => o.EmoteId == _emote.ToString() && o.UseCount > 0);

            var queryData = baseQuery.GroupBy(o => o.EmoteId).Select(o => new
            {
                UsersCount = o.Count(),
                FirstOccurence = o.Min(x => x.FirstOccurence),
                LastOccurence = o.Max(x => x.LastOccurence),
                UseCount = o.Sum(x => x.UseCount)
            });

            var data = await queryData.FirstOrDefaultAsync();
            var topTenQuery = baseQuery.OrderByDescending(x => x.UseCount).ThenByDescending(x => x.LastOccurence).Take(10);

            var topTen = (await topTenQuery.ToListAsync()).Select((o, i) =>
            {
                var user = Context.Client.FindUserAsync(Convert.ToUInt64(o.UserId)).Result;
                return $"**{i + 1,2}.** {user.GetDisplayName()} ({o.UseCount})";
            });

            var embed = new EmbedBuilder()
                .WithFooter(Context.User)
                .WithAuthor("Statistika použití emote")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithTitle(_emote.ToString())
                .AddField("Název", _emote.Name, true)
                .AddField("Animován", FormatHelper.FormatBooleanToCzech(_emote.Animated), true)
                .AddField("První výskyt", data.FirstOccurence.ToCzechFormat(), true)
                .AddField("Poslední výskyt", data.LastOccurence.ToCzechFormat(), true)
                .AddField("Od posl. použití", (DateTime.Now - data.LastOccurence).Humanize(culture: new CultureInfo("cs-CZ")), true)
                .AddField("Počet použití", data.UseCount, true)
                .AddField("Počet uživatelů", data.UsersCount, true)
                .AddField("TOP 10 použití", string.Join("\n", topTen), false)
                .AddField("Odkaz", _emote.Url, false);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
