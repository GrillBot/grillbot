using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class UnverifyRepository : RepositoryBase
{
    public UnverifyRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ulong>> GetUserIdsWithUnverify(IGuild guild)
    {
        using (Counter.Create("Database"))
        {
            var ids = await Context.Unverifies.AsNoTracking()
                .Where(o => o.GuildId == guild.Id.ToString())
                .Select(o => o.UserId)
                .ToListAsync();

            return ids.ConvertAll(o => o.ToUlong());
        }
    }

    public async Task<(int unverify, int selfunverify)> GetUserStatsAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var baseQuery = Context.UnverifyLogs.AsNoTracking()
                .Where(o => o.ToUserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());

            var unverify = await baseQuery.CountAsync(o => o.Operation == UnverifyOperation.Selfunverify);
            var selfunverify = await baseQuery.CountAsync(o => o.Operation == UnverifyOperation.Unverify);

            return (unverify, selfunverify);
        }
    }
}
