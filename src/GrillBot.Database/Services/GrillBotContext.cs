using GrillBot.Core.Database.ValueConverters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GrillBot.Database.Services;

public class GrillBotContext : DbContext
{
    public GrillBotContext(DbContextOptions options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildUser>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.User).WithMany(o => o.Guilds);
            builder.HasOne(o => o.Guild).WithMany(o => o.Users);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(o => o.SelfUnverifyMinimalTime).HasConversion(
                o => o.HasValue ? o.Value.ToString("c") : null,
                o => !string.IsNullOrEmpty(o) ? TimeSpan.Parse(o) : null
            );
        });

        modelBuilder.Entity<GuildUserChannel>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.ChannelId, o.UserId });
            builder.HasOne(o => o.Guild).WithMany().HasForeignKey(o => o.GuildId);
            builder.HasOne(o => o.User).WithMany(o => o.Channels).HasForeignKey(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.Channel).WithMany(o => o.Users).HasForeignKey(o => new { o.GuildId, o.ChannelId });
        });

        modelBuilder.Entity<GuildChannel>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.ChannelId });
            builder.HasOne(o => o.Guild).WithMany(o => o.Channels);
            builder.HasOne(o => o.ParentChannel).WithMany().HasForeignKey(o => new { o.GuildId, o.ParentChannelId });
        });

        modelBuilder.Entity<UnverifyLog>(builder =>
        {
            builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
            builder.HasOne(o => o.ToUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ToUserId });
            builder.HasOne(o => o.Guild).WithMany();
        });

        modelBuilder.Entity<SelfunverifyKeepable>(builder => builder.HasKey(o => new { o.GroupName, o.Name }));
        modelBuilder.Entity<ApiClient>(builder => builder.Property(o => o.AllowedMethods).HasConversion(new JsonValueConverter<List<string>>()));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<GuildUser> GuildUsers => Set<GuildUser>();
    public DbSet<GuildChannel> Channels => Set<GuildChannel>();
    public DbSet<GuildUserChannel> UserChannels => Set<GuildUserChannel>();
    public DbSet<UnverifyLog> UnverifyLogs => Set<UnverifyLog>();
    public DbSet<SelfunverifyKeepable> SelfunverifyKeepables => Set<SelfunverifyKeepable>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
}
