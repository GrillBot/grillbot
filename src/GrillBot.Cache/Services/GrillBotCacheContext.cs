using GrillBot.Cache.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services;

public class GrillBotCacheContext : DbContext
{
    public GrillBotCacheContext(DbContextOptions<GrillBotCacheContext> options) : base(options)
    {
    }

    public DbSet<MessageIndex> MessageIndex => Set<MessageIndex>();
}
