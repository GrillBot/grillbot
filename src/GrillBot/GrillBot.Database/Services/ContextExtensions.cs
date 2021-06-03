using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GrillBot.Database.Services
{
    static public class ContextExtensions
    {
        static public async Task InitGuildAsync(this GrillBotContext context, string guildId)
        {
            if (!await context.Guilds.AnyAsync(o => o.Id == guildId))
                await context.AddAsync(new Guild() { Id = guildId });
        }

        static public async Task InitUserAsync(this GrillBotContext context, string userId)
        {
            if (!await context.Users.AnyAsync(o => o.Id == userId))
                await context.AddAsync(new User() { Id = userId });
        }

        static public async Task InitGuildUserAsync(this GrillBotContext context, string guildId, string userId)
        {
            if (!await context.GuildUsers.AnyAsync(o => o.GuildId == guildId && o.UserId == userId))
                await context.AddAsync(new GuildUser() { GuildId = guildId, UserId = userId });
        }

        static public async Task InitGuildChannelAsync(this GrillBotContext context, string guildId, string channelId, string userId)
        {
            if (!await context.Channels.AnyAsync(o => o.Id == channelId && o.GuildId == guildId && o.UserId == userId))
                await context.AddAsync(new GuildChannel() { GuildId = guildId, Id = channelId, UserId = userId });
        }
    }
}
