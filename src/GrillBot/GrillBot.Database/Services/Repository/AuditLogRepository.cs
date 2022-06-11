using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AuditLogRepository : RepositoryBase
{
    public AuditLogRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ulong>> GetDiscordAuditLogIdsAsync(IGuild? guild, IChannel? channel, AuditLogItemType[]? types, DateTime after)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.AuditLogs.AsNoTracking()
                .Where(o => o.DiscordAuditLogItemId != null && o.CreatedAt >= after);

            if (guild != null)
                query = query.Where(o => o.GuildId != guild.Id.ToString());

            if (channel != null)
                query = query.Where(o => o.ChannelId == channel.Id.ToString());

            if (types?.Length > 0)
                query = query.Where(o => types.Contains(o.Type));

            var ids = await query
                .Select(o => o.DiscordAuditLogItemId!)
                .ToListAsync();

            return ids
                .SelectMany(o => o.Split(','))
                .Select(o => o.Trim().ToUlong())
                .Distinct()
                .ToList();
        }
    }
}
