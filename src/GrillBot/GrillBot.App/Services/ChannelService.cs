using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Helpers;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services
{
    public class ChannelService : ServiceBase
    {
        private string CommandPrefix { get; }
        private MessageCache.MessageCache MessageCache { get; }

        public ChannelService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
            MessageCache.MessageCache messageCache) : base(client, dbFactory)
        {
            CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
            MessageCache = messageCache;

            DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
            DiscordClient.ChannelDestroyed += (channel) => channel is SocketTextChannel chnl ? OnGuildChannelRemovedAsync(chnl) : Task.CompletedTask;
            DiscordClient.MessageDeleted += OnMessageRemovedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            int argPos = 0;

            // Commands and DM in channelboard is not allowed.
            if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return;
            if (message.Channel is not SocketTextChannel textChannel) return;

            using var dbContext = DbFactory.Create();

            var guildId = textChannel.Guild.Id.ToString();
            var channelId = textChannel.Id.ToString();
            var userId = message.Author.Id.ToString();

            // Check DB for consistency.
            await dbContext.InitGuildAsync(textChannel.Guild, CancellationToken.None);
            await dbContext.InitUserAsync(message.Author, CancellationToken.None);
            await dbContext.InitGuildUserAsync(textChannel.Guild, message.Author as IGuildUser, CancellationToken.None);
            var channelType = DiscordHelper.GetChannelType(textChannel);
            await dbContext.InitGuildChannelAsync(textChannel.Guild, textChannel, channelType.Value, CancellationToken.None);

            // Search specific channel for specific guild and user.
            var channel = await dbContext.UserChannels.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.Id == channelId && o.UserId == userId);

            if (channel == null)
            {
                channel = new GuildUserChannel()
                {
                    UserId = userId,
                    GuildId = guildId,
                    Id = channelId,
                    FirstMessageAt = DateTime.Now,
                    Count = 0
                };

                await dbContext.AddAsync(channel);
            }

            channel.Count++;
            channel.LastMessageAt = DateTime.Now;
            await dbContext.SaveChangesAsync();
        }

        private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
        {
            var msg = message.HasValue ? message.Value : MessageCache.GetMessage(message.Id);
            if (!messageChannel.HasValue || msg == null || messageChannel.Value is not SocketTextChannel channel) return;

            var guildId = channel.Guild.Id.ToString();
            var userId = msg.Author.Id.ToString();
            var channelId = channel.Id.ToString();

            using var dbContext = DbFactory.Create();

            var dbChannel = await dbContext.UserChannels.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId && o.Id == channelId);

            if (dbChannel == null) return;

            dbChannel.Count--;
            await dbContext.SaveChangesAsync();
        }

        private async Task OnGuildChannelRemovedAsync(SocketTextChannel channel)
        {
            var guildId = channel.Guild.Id.ToString();
            var channelId = channel.Id.ToString();

            using var dbContext = DbFactory.Create();

            var channelsQuery = dbContext.UserChannels.AsQueryable().Where(o => o.Id == channelId && o.GuildId == guildId);
            var channels = await channelsQuery.ToListAsync();

            dbContext.RemoveRange(channels);
            await dbContext.SaveChangesAsync();
        }

        public async Task<SocketTextChannel> GetMostActiveChannelOfUserAsync(IUser user, IGuild guild)
        {
            using var dbContext = DbFactory.Create();

            var channelIdQuery = dbContext.UserChannels.AsNoTracking()
                .OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt)
                .Where(o => o.Channel.ChannelType == ChannelType.Text && o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString())
                .Select(o => o.Id);
            var channelId = await channelIdQuery.FirstOrDefaultAsync();

            // User not have any active channel.
            if (string.IsNullOrEmpty(channelId)) return null;
            return (await guild.GetTextChannelAsync(Convert.ToUInt64(channelId))) as SocketTextChannel;
        }

        public async Task<IUserMessage> GetLastMsgFromMostActiveChannelAsync(SocketGuild guild, IUser loggedUser)
        {
            // Using statistics and finding most active channel will help find channel where logged user have any message.
            // This eliminates the need to browser channels and finds some activity.
            var mostActiveChannel = await GetMostActiveChannelOfUserAsync(loggedUser, guild);
            if (mostActiveChannel == null) return null;

            return await TryFindLastMessageFromUserAsync(mostActiveChannel, loggedUser, true);
        }

        private async Task<IUserMessage> TryFindLastMessageFromUserAsync(SocketTextChannel channel, IUser loggedUser, bool canTryRepeat)
        {
            var lastMessage = new[]
            {
                channel.CachedMessages.Where(o => o.Author.Id == loggedUser.Id).OrderByDescending(o => o.Id).FirstOrDefault(),
                MessageCache.GetLastMessageFromUserInChannel(channel, loggedUser)
            }.Where(o => o != null).OrderByDescending(o => o.Id).FirstOrDefault();

            if (lastMessage == null && canTryRepeat)
            {
                await MessageCache.DownloadLatestFromChannelAsync(channel);
                return await TryFindLastMessageFromUserAsync(channel, loggedUser, false);
            }

            return lastMessage as IUserMessage;
        }
    }
}
