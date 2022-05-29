using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Database.Services.Repository;

public class ChannelRepository : RepositoryBase
{
    public ChannelRepository(GrillBotContext dbContext) : base(dbContext)
    {
    }

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong guildId, ulong channelId, bool disableTracking = false)
    {
        var query = Context.Channels.Include(o => o.Users).AsQueryable();

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.ChannelId == channelId.ToString());
    }
}
