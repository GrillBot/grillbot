using Discord;
using GrillBot.Cache.Entity;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class InviteMetadataRepository : RepositoryBase<GrillBotCacheContext>
{
    public InviteMetadataRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<InviteMetadata>> GetInvitesOfGuildAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            return await Context.InviteMetadata
                .Where(o => o.GuildId == guild.Id.ToString())
                .ToListAsync();
        }
    }

    public async Task<int> GetCountAsync()
    {
        using (CreateCounter())
        {
            return await Context.InviteMetadata.CountAsync();
        }
    }

    public async Task<bool> InviteExistsAsync(IGuild guild, IInviteMetadata metadata)
    {
        using (CreateCounter())
        {
            return await Context.InviteMetadata.AsNoTracking()
                .AnyAsync(o => o.GuildId == guild.Id.ToString() && o.Code == metadata.Code);
        }
    }

    public void DeleteAllInvites()
    {
        using (CreateCounter())
        {
            Context.Database.ExecuteSqlRaw("DELETE FROM public.\"InviteMetadata\"");
        }
    }
}
