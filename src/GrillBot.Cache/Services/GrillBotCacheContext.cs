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
        modelBuilder.Entity<InviteMetadata>(builder => builder.HasKey(o => new { o.GuildId, o.Code }));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<MessageIndex> MessageIndex => Set<MessageIndex>();
    public DbSet<ProfilePicture> ProfilePictures => Set<ProfilePicture>();
    public DbSet<InviteMetadata> InviteMetadata => Set<InviteMetadata>();
    public DbSet<DataCacheItem> DataCache => Set<DataCacheItem>();
}
