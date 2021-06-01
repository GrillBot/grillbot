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
    public class PointsService : ServiceBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private string CommandPrefix { get; }
        private IConfiguration Configuration { get; }
        private Random Random { get; }

        public PointsService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration) : base(client)
        {
            DbFactory = dbFactory;
            CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
            Configuration = configuration.GetSection("Points");
            Random = new Random();

            DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
            DiscordClient.ReactionAdded += OnReactionAddedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            if (!CanIncrement(message)) return;
            if (message.Channel is not SocketTextChannel textChannel) return;

            var guildId = textChannel.Guild.Id.ToString();
            var userId = message.Author.Id.ToString();

            using var context = DbFactory.Create();

            if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId))
                await context.AddAsync(new Guild() { Id = guildId });

            if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId))
                await context.Users.AddAsync(new User() { Id = userId });

            var guildUser = await context.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

            if (guildUser == null)
            {
                guildUser = new GuildUser()
                {
                    GuildId = guildId,
                    UserId = userId
                };

                await context.AddAsync(guildUser);
            }

            IncrementPoints(
                guildUser,
                user => user.LastPointsMessageIncrement,
                Configuration.GetSection("Range:Message"),
                Configuration.GetValue<int>("Cooldown:Message"),
                user => user.LastPointsMessageIncrement = DateTime.Now
            );

            await context.SaveChangesAsync();
        }

        private bool CanIncrement(IUserMessage message)
        {
            int argPos = 0;

            if (message == null) return false;
            if (string.IsNullOrEmpty(message.Content)) return false;
            if (message.Content.Length < Configuration.GetValue<int>("MessageMinLength")) return false;
            if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return false;

            return true;
        }

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is not SocketTextChannel textChannel) return; // Only guilds
            if (reaction.Emote is not Emoji && !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return; // Only local emotes.

            await textChannel.Guild.DownloadUsersAsync();
            var user = reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId);
            if (user?.IsUser() != true) return;

            int argPos = 0;
            var msg = await message.GetOrDownloadAsync();
            if (!CanIncrement(msg)) return;
            if (msg.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix) == true) return;

            var guildId = textChannel.Guild.Id.ToString();
            var userId = reaction.UserId.ToString();

            using var context = DbFactory.Create();

            if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId))
                await context.AddAsync(new Guild() { Id = guildId });

            if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId))
                await context.Users.AddAsync(new User() { Id = userId });

            var guildUser = await context.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

            if (guildUser == null)
            {
                guildUser = new GuildUser()
                {
                    GuildId = guildId,
                    UserId = userId
                };

                await context.AddAsync(guildUser);
            }

            IncrementPoints(
                guildUser,
                guildUser => guildUser.LastPointsReactionIncrement,
                Configuration.GetSection("Range:Reaction"),
                Configuration.GetValue<int>("Cooldown:Reaction"),
                guildUser => guildUser.LastPointsReactionIncrement = DateTime.Now
            );

            await context.SaveChangesAsync();
        }

        private void IncrementPoints(GuildUser user, Func<GuildUser, DateTime?> lastIncrementSelector, IConfiguration range, int cooldown, Action<GuildUser> lastIncrementReset)
        {
            var lastIncrement = lastIncrementSelector(user);

            if (lastIncrement.HasValue && lastIncrement.Value.AddSeconds(cooldown) > DateTime.Now)
                return;

            user.Points += Random.Next(range.GetValue<int>("From"), range.GetValue<int>("To"));
            lastIncrementReset(user);
        }
    }
}
