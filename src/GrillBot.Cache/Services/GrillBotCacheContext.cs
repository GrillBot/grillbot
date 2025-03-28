﻿using GrillBot.Cache.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services;

public class GrillBotCacheContext : DbContext
{
    public GrillBotCacheContext(DbContextOptions<GrillBotCacheContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InviteMetadata>(builder => builder.HasKey(o => new { o.GuildId, o.Code }));
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<MessageIndex> MessageIndex => Set<MessageIndex>();
    public DbSet<InviteMetadata> InviteMetadata => Set<InviteMetadata>();
}
