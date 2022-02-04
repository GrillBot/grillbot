using GrillBot.App.Extensions;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using System.Text.RegularExpressions;

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
            var messageContent = ParseAndCheckMessage(message);

            using var context = DbFactory.Create();

            await context.InitUserAsync(user, CancellationToken.None);
            await context.InitGuildAsync(guild, CancellationToken.None);
            await context.InitGuildChannelAsync(guild, channel, DiscordHelper.GetChannelType(channel).Value, CancellationToken.None);

            var entity = new SearchItem()
            {
                ChannelId = channel.Id.ToString(),
                GuildId = guild.Id.ToString(),
                UserId = user.Id.ToString(),
                JumpUrl = message.GetJumpUrl(),
                MessageContent = messageContent
            };

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        private string ParseAndCheckMessage(IUserMessage message)
        {
            var messageContent = GetMessageContent(message);
            if (string.IsNullOrEmpty(messageContent))
                throw new ValidationException("Obsah zprávy nesmí být prázdný.");

            if (messageContent.Length > EmbedFieldBuilder.MaxFieldValueLength)
                throw new ValidationException($"Zpráva nesmí být delší, než {EmbedFieldBuilder.MaxFieldValueLength} znaků.");

            return messageContent;
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

            var data = await GetPaginatedListAsync(parameters, CancellationToken.None);

            return data.Data.ConvertAll(o => new SearchingItem()
            {
                JumpLink = o.JumpLink,
                DisplayName = o.User.Username,
                Id = o.Id,
                Message = o.Message
            });
        }

        public async Task<PaginatedResponse<SearchingListItem>> GetPaginatedListAsync(GetSearchingListParams parameters, CancellationToken cancellationToken)
        {
            using var context = DbFactory.Create();

            var query = context.SearchItems.AsSplitQuery()
                .Include(o => o.Channel)
                .Include(o => o.Guild)
                .Include(o => o.User)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var data = await query.ToListAsync(cancellationToken);

            var results = new List<SearchingListItem>();
            var synchronizedGuilds = new List<ulong>();

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

                if (!synchronizedGuilds.Contains(guild.Id))
                {
                    await guild.DownloadUsersAsync();
                    synchronizedGuilds.Add(guild.Id);
                }

                var author = guild.GetUser(Convert.ToUInt64(item.UserId));
                if (author == null)
                {
                    CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                    continue;
                }

                if (string.IsNullOrEmpty(item.MessageContent))
                {
                    // Old version. Download message, store to entity/db and return object. Next load will take from DB.
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

                    item.MessageContent = messageContent.Cut(EmbedFieldBuilder.MaxFieldValueLength);
                    item.JumpUrl = message.GetJumpUrl();
                    item.MessageId = null;
                }

                results.Add(new SearchingListItem(item));
            }

            await context.SaveChangesAsync(cancellationToken);
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
