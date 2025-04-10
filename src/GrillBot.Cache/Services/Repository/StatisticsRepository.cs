﻿using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class StatisticsRepository : SubRepositoryBase<GrillBotCacheContext>
{
    public StatisticsRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<Dictionary<string, int>> GetTableStatisticsAsync()
    {
        using (CreateCounter())
        {
            return new Dictionary<string, int>
            {
                { nameof(DbContext.MessageIndex), await DbContext.MessageIndex.CountAsync() }
            };
        }
    }
}
