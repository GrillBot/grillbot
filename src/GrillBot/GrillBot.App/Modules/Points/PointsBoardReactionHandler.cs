using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Points
{
    public class PointsBoardReactionHandler : ReactionEventHandler
    {
        private GrillBotContextFactory DbFactory { get; }
        private DiscordSocketClient DiscordClient { get; }

        public PointsBoardReactionHandler(GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
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
            if (!embed.TryParseMetadata<PointsBoardMetadata>(out var metadata)) return false;

            var guild = DiscordClient.GetGuild(metadata.GuildId);
            if (guild == null) return false;

            var dbContext = DbFactory.Create();

            var query = dbContext.GuildUsers.AsQueryable()
                .Where(o => o.GuildId == guild.Id.ToString() && o.Points > 0)
                .OrderByDescending(o => o.Points)
                .Select(o => new KeyValuePair<string, long>(o.UserId, o.Points));

            var pointsCount = await query.CountAsync();
            if (pointsCount == 0) return false;
            var pagesCount = (int)Math.Floor(pointsCount / 10.0);

            int newPage = metadata.PageNumber;
            if (emote.IsEqual(Emojis.MoveToFirst)) newPage = 0;
            else if (emote.IsEqual(Emojis.MoveToLast)) newPage = pagesCount - 1;
            else if (emote.IsEqual(Emojis.MoveToNext)) newPage++;
            else if (emote.IsEqual(Emojis.MoveToPrev)) newPage--;

            if (newPage >= pagesCount) newPage = pagesCount - 1;
            else if (newPage < 0) newPage = 0;
            if (newPage == metadata.PageNumber) return false;

            var skip = (newPage == 0 ? 0 : newPage) * 10;
            var filteredQuery = query.Skip(skip).Take(10);
            var data = await filteredQuery.ToListAsync();

            await guild.DownloadUsersAsync();
            var resultEmbed = new PointsBoardBuilder()
                .WithBoard(user, guild, data, id => guild.GetUser(id), skip, newPage);

            await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
            await message.RemoveReactionAsync(emote, user);

            return true;
        }
    }
}
