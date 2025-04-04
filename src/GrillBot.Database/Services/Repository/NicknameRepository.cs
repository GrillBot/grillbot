﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class NicknameRepository : SubRepositoryBase<GrillBotContext>
{
    public NicknameRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<long> ComputeIdAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var maxRecord = await DbContext.Nicknames.AsNoTracking()
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
            return await DbContext.Nicknames.AsNoTracking()
                .AnyAsync(o => o.GuildId == user.GuildId.ToString() && o.UserId == user.Id.ToString() && o.NicknameValue == user.Nickname);
        }
    }

    public async Task<bool> ExistsAnyNickname(IGuildUser user)
    {
        using (CreateCounter())
        {
            return await DbContext.Nicknames.AsNoTracking()
                .AnyAsync(o => o.GuildId == user.GuildId.ToString() && o.UserId == user.Id.ToString());
        }
    }
}
