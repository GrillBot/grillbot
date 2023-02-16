using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class NicknameRepository : RepositoryBase
{
    public NicknameRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<long> ComputeIdAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var maxRecord = await Context.Nicknames.AsNoTracking()
                .Where(o => o.GuildId == user.GuildId.ToString() && o.UserId == user.Id.ToString())
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            return (maxRecord?.Id ?? 0) + 1;
        }
    }

    public async Task<bool> ExistsAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            return await Context.Nicknames.AsNoTracking()
                .AnyAsync(o => o.GuildId == user.GuildId.ToString() && o.UserId == user.Id.ToString() && o.NicknameValue == user.Nickname);
        }
    }
}
