using Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GrillBot.Database.Services
{
    static public class ContextExtensions
    {
        static public async Task InitGuildAsync(this GrillBotContext context, IGuild guild)
        {
            var guildId = guild.Id.ToString();

            if (!await context.Guilds.AnyAsync(o => o.Id == guildId))
                await context.AddAsync(Guild.FromDiscord(guild));
        }

        static public async Task InitUserAsync(this GrillBotContext context, IUser user)
        {
            var userId = user.Id.ToString();

            if (!await context.Users.AnyAsync(o => o.Id == userId))
                await context.AddAsync(User.FromDiscord(user));
        }

        static public async Task InitGuildUserAsync(this GrillBotContext context, IGuild guild, IGuildUser user)
        {
            var userId = user.Id.ToString();
            var guildId = guild.Id.ToString();

            if (!await context.GuildUsers.AnyAsync(o => o.GuildId == guildId && o.UserId == userId))
                await context.AddAsync(GuildUser.FromDiscord(guild, user));
        }

        static public async Task InitGuildChannelAsync(this GrillBotContext context, IGuild guild, IChannel channel, ChannelType channelType)
        {
            var channelId = channel.Id.ToString();
            var guildId = guild.Id.ToString();

            if (!await context.Channels.AnyAsync(o => o.ChannelId == channelId && o.GuildId == guildId))
                await context.AddAsync(GuildChannel.FromDiscord(guild, channel, channelType));
        }
    }
}
