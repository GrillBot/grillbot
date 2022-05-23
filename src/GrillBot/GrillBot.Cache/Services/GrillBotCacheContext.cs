using GrillBot.Cache.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services;

public class GrillBotCacheContext : DbContext
{
    public GrillBotCacheContext(DbContextOptions<GrillBotCacheContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProfilePicture>(builder => builder.HasKey(o => new { o.UserId, o.Size, o.AvatarId }));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<MessageIndex> MessageIndex => Set<MessageIndex>();
    public DbSet<DirectApiMessage> DirectApiMessages => Set<DirectApiMessage>();
    public DbSet<ProfilePicture> ProfilePictures => Set<ProfilePicture>();
}
