using Discord;
using Discord.WebSocket;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class SearchingService : ServiceBase
    {
        private MessageCache.MessageCache MessageCache { get; }
        private Regex EmptyMessageRegex { get; }

        public SearchingService(DiscordSocketClient client, GrillBotContextFactory dbFactory, MessageCache.MessageCache messageCache) : base(client, dbFactory)
        {
            MessageCache = messageCache;

            EmptyMessageRegex = new Regex(@"(^.)hledam(\s*add)?$");
        }

        public async Task CreateAsync(IGuild guild, IUser user, IChannel channel, IUserMessage message)
        {
            using var context = DbFactory.Create();

            await context.InitUserAsync(user, CancellationToken.None);
            await context.InitGuildAsync(guild, CancellationToken.None);
            await context.InitGuildChannelAsync(guild, channel, DiscordHelper.GetChannelType(channel).Value, CancellationToken.None);

            var entity = new SearchItem()
            {
                ChannelId = channel.Id.ToString(),
                GuildId = guild.Id.ToString(),
                MessageId = message.Id.ToString(),
                UserId = user.Id.ToString()
            };

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task RemoveSearchAsync(long id, IUser executor, bool admin)
        {
            using var context = DbFactory.Create();

            var search = await context.SearchItems.AsQueryable().FirstOrDefaultAsync(o => o.Id == id);
            if (search == null) return;

            if (!admin && executor.Id != Convert.ToUInt64(search.UserId))
                throw new UnauthorizedAccessException("Na provedení tohoto příkazu nemáš oprávnění.");

            context.Remove(search);
            await context.SaveChangesAsync();
        }

        public async Task<List<SearchingItem>> GetSearchListAsync(SocketGuild guild, ISocketMessageChannel channel, int page)
        {
            var parameters = new GetSearchingListParams()
            {
                ChannelId = channel.Id.ToString(),
                GuildId = guild.Id.ToString(),
                Page = page,
                PageSize = EmbedBuilder.MaxFieldCount,
                SortDesc = false,
                SortBy = "Id"
            };

            var data = await GetPaginatedListAsync(parameters);

            return data.Data.ConvertAll(o => new SearchingItem()
            {
                JumpLink = o.JumpLink,
                DisplayName = o.User.Username,
                Id = o.Id,
                Message = o.Message
            });
        }

        public async Task<PaginatedResponse<SearchingListItem>> GetPaginatedListAsync(GetSearchingListParams parameters)
        {
            using var context = DbFactory.Create();

            var query = context.SearchItems.AsNoTracking()
                .Include(o => o.Channel)
                .Include(o => o.Guild)
                .Include(o => o.User)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var data = await query.ToListAsync();

            var results = new List<SearchingListItem>();
            foreach (var item in data)
            {
                var guild = DiscordClient.GetGuild(Convert.ToUInt64(item.GuildId));
                if (guild == null)
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                var channel = guild.GetTextChannel(Convert.ToUInt64(item.ChannelId));
                if (channel == null)
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                await guild.DownloadUsersAsync();
                var author = guild.GetUser(Convert.ToUInt64(item.UserId));
                if (author == null)
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                var message = await MessageCache.GetMessageAsync(channel, Convert.ToUInt64(item.MessageId));
                if (message == null)
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                var messageContent = GetMessageContent(message);
                if (string.IsNullOrEmpty(messageContent))
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                results.Add(new SearchingListItem(item, messageContent, message.GetJumpUrl()));
            }

            await context.SaveChangesAsync();
            return PaginatedResponse<SearchingListItem>.Create(results, parameters);
        }

        private string GetMessageContent(IMessage message)
        {
            if (string.IsNullOrEmpty(message?.Content) || EmptyMessageRegex.IsMatch(message.Content)) return null;

            if (message.Content[1..].StartsWith("hledam add"))
                return message.Content[("hledam add".Length + 1)..].Trim();

            if (message.Content[1..].StartsWith("hledam"))
                return message.Content[("hledam".Length + 1)..].Trim();

            return message.Content.Trim();
        }
    }
}
