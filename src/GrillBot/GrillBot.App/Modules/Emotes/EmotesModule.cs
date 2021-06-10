using Discord;
using Discord.Commands;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Data;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Emotes
{
    [Group("emote")]
    [Summary("Správa emotů")]
    public class EmotesModule : Infrastructure.ModuleBase
    {
        [Group("list")]
        [Summary("Získání seznamu statistiky emotů")]
        public class EmoteListSubModule : Infrastructure.ModuleBase
        {
            [Command]
            [Summary("Získání seznamu statistiky emotů podle počtu použití.")]
            public Task<RuntimeResult> GetListByCount(IUser user = null) => Task.FromResult(new CommandRedirectResult($"emote list count desc {user?.Mention}".Trim()) as RuntimeResult);

            [Group("count")]
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
                public async Task GetDescendingListByCount(IUser user = null)
                {
                    using var dbContext = DbFactory.Create();

                    var query = GetListQuery(dbContext, user?.Id, o => o.OrderByDescending(x => x.Sum(t => t.UseCount)).ThenByDescending(o => o.Max(x => x.LastOccurence)),
                        0, EmbedBuilder.MaxFieldCount);
                    var data = await query.ToListAsync();

                    var list = new EmbedBuilder().WithEmoteList(data, Context.User, user, Context.IsPrivate, true, "count", 0);
                    var message = await ReplyAsync(embed: list.Build());

                    if (data.Count > 0)
                        await message.AddReactionsAsync(Emojis.PaginationEmojis);
                }

                [Command("asc")]
                [Summary("Získání seznamu statistiky emotů podle počtu použití vzestupně.")]
                public async Task GetAscendingListByCount(IUser user = null)
                {
                    using var dbContext = DbFactory.Create();

                    var query = GetListQuery(dbContext, user?.Id, o => o.OrderBy(x => x.Sum(t => t.UseCount)).ThenBy(o => o.Max(x => x.LastOccurence)),
                        0, EmbedBuilder.MaxFieldCount);
                    var data = await query.ToListAsync();

                    var list = new EmbedBuilder().WithEmoteList(data, Context.User, user, Context.IsPrivate, false, "count", 0);
                    var message = await ReplyAsync(embed: list.Build());

                    if (data.Count > 0)
                        await message.AddReactionsAsync(Emojis.PaginationEmojis);
                }
            }

            [Group("lastuse")]
            [Summary("Získání seznamu statistiky emotů podle data posledního použití.")]
            public class EmoteListByLastUseSubModule : Infrastructure.ModuleBase
            {
                [Command("desc")]
                [Summary("Získání seznamu statistiky emotů podle data posledního použití sestupně.")]
                public async Task GetDescendingListByCount(IUser user = null) { }

                [Command("asc")]
                [Summary("Získání seznamu statistiky emotů podle data posledního použití vzestupně.")]
                public async Task GetAscendingListByCount(IUser user = null) { }
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
    }
}
