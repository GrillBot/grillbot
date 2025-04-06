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

        modelBuilder.Entity<Unverify>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.GuildUser).WithOne(o => o.Unverify).HasForeignKey<Unverify>(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.UnverifyLog).WithOne(o => o.Unverify);
            builder.HasOne(o => o.Guild).WithMany(o => o.Unverifies);
            builder.Property(o => o.Roles).HasConversion(new JsonValueConverter<List<string>>());
            builder.Property(o => o.Channels).HasConversion(new JsonValueConverter<List<GuildChannelOverride>>());
        });

        modelBuilder.Entity<UnverifyLog>(builder =>
        {
            builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
            builder.HasOne(o => o.ToUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ToUserId });
            builder.HasOne(o => o.Guild).WithMany(o => o.UnverifyLogs);
        });

        modelBuilder.Entity<SelfunverifyKeepable>(builder => builder.HasKey(o => new { o.GroupName, o.Name }));

        modelBuilder.Entity<Nickname>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId, o.Id });
            builder.HasOne(o => o.User).WithMany(o => o.Nicknames).HasForeignKey(o => new { o.GuildId, o.UserId });
        });

        modelBuilder.Entity<ApiClient>(builder => builder.Property(o => o.AllowedMethods).HasConversion(new JsonValueConverter<List<string>>()));

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<GuildUser> GuildUsers => Set<GuildUser>();
    public DbSet<GuildChannel> Channels => Set<GuildChannel>();
    public DbSet<GuildUserChannel> UserChannels => Set<GuildUserChannel>();
    public DbSet<Unverify> Unverifies => Set<Unverify>();
    public DbSet<UnverifyLog> UnverifyLogs => Set<UnverifyLog>();
    public DbSet<SelfunverifyKeepable> SelfunverifyKeepables => Set<SelfunverifyKeepable>();
    public DbSet<AutoReplyItem> AutoReplies => Set<AutoReplyItem>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<Nickname> Nicknames => Set<Nickname>();
}
