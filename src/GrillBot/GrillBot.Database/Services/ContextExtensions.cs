using Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Database.Services
{
    static public class ContextExtensions
    {
        static public async Task InitGuildAsync(this GrillBotContext context, IGuild guild, CancellationToken cancellationToken = default)
        {
            var guildId = guild.Id.ToString();

            if (!await context.Guilds.AnyAsync(o => o.Id == guildId, cancellationToken))
                await context.AddAsync(Guild.FromDiscord(guild), cancellationToken);
        }

        static public async Task InitUserAsync(this GrillBotContext context, IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null) return;
            var userId = user.Id.ToString();

            if (!await context.Users.AnyAsync(o => o.Id == userId, cancellationToken))
                await context.AddAsync(User.FromDiscord(user), cancellationToken);
        }

        static public async Task InitGuildUserAsync(this GrillBotContext context, IGuild guild, IGuildUser user, CancellationToken cancellationToken = default)
        {
            var userId = user.Id.ToString();
            var guildId = guild.Id.ToString();

            if (!await context.GuildUsers.AnyAsync(o => o.GuildId == guildId && o.UserId == userId, cancellationToken))
                await context.AddAsync(GuildUser.FromDiscord(guild, user), cancellationToken);
        }

        static public async Task InitGuildUserAsync(this GrillBotContext context, ulong guildId, ulong userId, CancellationToken cancellationToken)
        {
            var _userId = userId.ToString();
            var _guildId = guildId.ToString();

            if (!await context.GuildUsers.AnyAsync(o => o.GuildId == _guildId && o.UserId == _userId, cancellationToken))
                await context.AddAsync(new GuildUser() { GuildId = _guildId, UserId = _userId }, cancellationToken);
        }

        static public async Task InitGuildChannelAsync(this GrillBotContext context, IGuild guild, IChannel channel, ChannelType channelType,
            CancellationToken cancellationToken = default)
        {
            var channelId = channel.Id.ToString();
            var guildId = guild.Id.ToString();

            if (!await context.Channels.AnyAsync(o => o.ChannelId == channelId && o.GuildId == guildId, cancellationToken))
                await context.AddAsync(GuildChannel.FromDiscord(guild, channel, channelType), cancellationToken);
        }
    }
}
