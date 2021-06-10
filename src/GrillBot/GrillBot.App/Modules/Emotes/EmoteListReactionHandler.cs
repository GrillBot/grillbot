using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Handlers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Emotes
{
    public class EmoteListReactionHandler : ReactionEventHandler
    {
        private GrillBotContextFactory DbFactory { get; }
        private DiscordSocketClient DiscordClient { get; }

        public EmoteListReactionHandler(GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
        {
            DbFactory = dbFactory;
            DiscordClient = discordClient;
        }

        public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
        {
            var embed = message.Embeds.FirstOrDefault();

            if (embed == null || embed.Footer == null || embed.Author == null) return false;
            if (!Emojis.PaginationEmojis.Any(o => o.IsEqual(emote))) return false;
            if (message.ReferencedMessage == null) return false;
            if (!embed.TryParseMetadata<EmoteListMetadata>(out var metadata)) return false;

            var sortFunc = GetOrderFunction(metadata.SortBy, metadata.Desc);
            if (sortFunc == null) return false;

            using var context = DbFactory.Create();

            var query = EmotesModule.EmoteListSubModule.GetListQuery(context, metadata.OfUserId, sortFunc, null, null);
            var emotesCount = await query.CountAsync();
            if (emotesCount == 0) return false;

            int maxPages = (int)Math.Ceiling(emotesCount / (double)EmbedBuilder.MaxFieldCount);
            int newPage = metadata.Page;
            if (emote.IsEqual(Emojis.MoveToFirst)) newPage = 0;
            else if (emote.IsEqual(Emojis.MoveToLast)) newPage = maxPages - 1;
            else if (emote.IsEqual(Emojis.MoveToNext)) newPage++;
            else if (emote.IsEqual(Emojis.MoveToPrev)) newPage--;

            if (newPage >= maxPages) newPage = maxPages - 1;
            else if (newPage < 0) newPage = 0;
            if (newPage == metadata.Page) return false;

            var skip = newPage * EmbedBuilder.MaxFieldCount;
            query = query.Skip(skip).Take(EmbedBuilder.MaxFieldCount);
            var data = await query.ToListAsync();

            var forUser = metadata.OfUserId == null ? null : await DiscordClient.FindUserAsync(metadata.OfUserId.Value);
            var resultEmbed = new EmbedBuilder()
                .WithEmoteList(data, user, forUser, metadata.IsPrivate, metadata.Desc, metadata.SortBy, newPage);

            await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
            if (!metadata.IsPrivate)
                await message.RemoveReactionAsync(emote, user);

            return false;
        }

        private Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> GetOrderFunction(string sortBy, bool desc)
        {
            return sortBy switch
            {
                "count" => desc switch
                {
                    true => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderByDescending(o => o.Sum(x => x.UseCount)).ThenByDescending(o => o.Max(x => x.LastOccurence)),
                    false => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderBy(o => o.Sum(x => x.UseCount)).ThenBy(o => o.Max(x => x.LastOccurence))
                },
                "lastuse" => desc switch
                {
                    true => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderByDescending(o => o.Max(x => x.LastOccurence)).ThenByDescending(o => o.Sum(x => x.UseCount)),
                    false => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderBy(o => o.Max(x => x.LastOccurence)).ThenBy(o => o.Sum(x => x.UseCount))
                },
                _ => null
            };
        }
    }
}
