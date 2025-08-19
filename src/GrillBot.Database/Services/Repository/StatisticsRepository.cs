using System.Collections.Generic;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class StatisticsRepository : SubRepositoryBase<GrillBotContext>
{
    public StatisticsRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<Dictionary<string, int>> GetTablesStatusAsync()
    {
        using (CreateCounter())
        {
            return new Dictionary<string, int>
            {
                { nameof(DbContext.Users), await DbContext.Users.CountAsync() },
                { nameof(DbContext.Guilds), await DbContext.Guilds.CountAsync() },
                { nameof(DbContext.GuildUsers), await DbContext.GuildUsers.CountAsync() },
                { nameof(DbContext.Channels), await DbContext.Channels.CountAsync() },
                { nameof(DbContext.UserChannels), await DbContext.UserChannels.CountAsync() },
                { nameof(DbContext.ApiClients), await DbContext.ApiClients.CountAsync() }
            };
        }
    }
}
