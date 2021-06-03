using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class ChannelService : ServiceBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private string CommandPrefix { get; }
        private MessageCache MessageCache { get; }

        public ChannelService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
            MessageCache messageCache) : base(client)
        {
            DbFactory = dbFactory;
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
            await dbContext.InitGuildAsync(guildId);
            await dbContext.InitUserAsync(userId);
            await dbContext.InitGuildUserAsync(guildId, userId);

            // Search specific channel for specific guild and user.
            var channel = await dbContext.Channels.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.Id == channelId && o.UserId == userId);

            if (channel == null)
            {
                channel = new GuildChannel()
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

        private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, ISocketMessageChannel messageChannel)
        {
            var msg = message.HasValue ? message.Value : MessageCache.GetMessage(message.Id);
            if (msg == null || messageChannel is not SocketTextChannel channel) return;

            var guildId = channel.Guild.Id.ToString();
            var userId = msg.Author.Id.ToString();
            var channelId = channel.Id.ToString();

            using var dbContext = DbFactory.Create();

            var dbChannel = await dbContext.Channels.AsQueryable()
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

            var channelsQuery = dbContext.Channels.AsQueryable().Where(o => o.Id == channelId && o.GuildId == guildId);
            var channels = await channelsQuery.ToListAsync();

            dbContext.RemoveRange(channels);
            await dbContext.SaveChangesAsync();
        }
    }
}
