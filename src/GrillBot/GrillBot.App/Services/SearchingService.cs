using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            await context.InitUserAsync(user);
            await context.InitGuildAsync(guild);
            await context.InitGuildChannelAsync(guild, channel, DiscordHelper.GetChannelType(channel).Value);

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

            var search = await context.SearchItems.FirstOrDefaultAsync(o => o.Id == id);
            if (search == null) return;

            if (!admin && executor.Id != Convert.ToUInt64(search.UserId))
                throw new UnauthorizedAccessException("Na provedení tohoto příkazu nemáš oprávnění.");

            context.Remove(search);
            await context.SaveChangesAsync();
        }

        public async Task<List<SearchingItem>> GetSearchListAsync(SocketGuild guild, ISocketMessageChannel channel, int page)
        {
            await guild.DownloadUsersAsync();

            using var context = DbFactory.Create();

            var searches = context.SearchItems.AsQueryable()
                .Where(o => o.GuildId == guild.Id.ToString() && o.ChannelId == channel.Id.ToString())
                .ToList();

            var results = new List<SearchingItem>();
            foreach (var search in searches)
            {
                var author = guild.GetUser(Convert.ToUInt64(search.UserId));
                if (author == null)
                {
                    context.Remove(search);
                    continue;
                }

                var message = await MessageCache.GetMessageAsync(channel, Convert.ToUInt64(search.MessageId));
                if (message == null)
                {
                    context.Remove(search);
                    continue;
                }

                var messageContent = GetMessageContent(message);
                if (string.IsNullOrEmpty(messageContent))
                {
                    context.Remove(search);
                    continue;
                }

                results.Add(new SearchingItem()
                {
                    DisplayName = author.GetDisplayName(),
                    Id = search.Id,
                    JumpLink = message.GetJumpUrl(),
                    Message = messageContent
                });
            }

            var skip = page * EmbedBuilder.MaxFieldCount;
            return results.Skip(skip).Take(EmbedBuilder.MaxFieldCount).ToList();
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
