using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;

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
            builder.HasOne(o => o.UsedInvite).WithMany(o => o.UsedUsers);
        });

        modelBuilder.Entity<Invite>(builder =>
        {
            builder.HasOne(o => o.Creator)
                .WithMany(o => o.CreatedInvites)
                .HasForeignKey(o => new { o.GuildId, o.CreatorId });

            builder.HasOne(o => o.Guild).WithMany(o => o.Invites);
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(o => o.SelfUnverifyMinimalTime).HasConversion(
                o => o.HasValue ? o.Value.ToString("c") : null,
                o => !string.IsNullOrEmpty(o) ? TimeSpan.Parse(o) : null
            );
        });

        modelBuilder.Entity<RemindMessage>(buider =>
        {
            buider.HasOne(o => o.FromUser).WithMany(o => o.OutgoingReminders);
            buider.HasOne(o => o.ToUser).WithMany(o => o.IncomingReminders);
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

        modelBuilder.Entity<SearchItem>(builder =>
        {
            builder.HasOne(o => o.User).WithMany(o => o.SearchItems);
            builder.HasOne(o => o.Channel).WithMany(o => o.SearchItems).HasForeignKey(o => new { o.GuildId, o.ChannelId });
            builder.HasOne(o => o.Guild).WithMany(o => o.Searches);
        });

        modelBuilder.Entity<Unverify>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.GuildUser).WithOne(o => o.Unverify).HasForeignKey<Unverify>(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.UnverifyLog).WithOne(o => o.Unverify);
            builder.HasOne(o => o.Guild).WithMany(o => o.Unverifies);
        });

        modelBuilder.Entity<UnverifyLog>(builder =>
        {
            builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
            builder.HasOne(o => o.ToUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ToUserId });
            builder.HasOne(o => o.Guild).WithMany(o => o.UnverifyLogs);
        });

        modelBuilder.Entity<SelfunverifyKeepable>(builder => builder.HasKey(o => new { o.GroupName, o.Name }));

        modelBuilder.Entity<EmoteSuggestion>(builder =>
        {
            builder.HasOne(o => o.Guild).WithMany();
            builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
        });

        modelBuilder.Entity<Nickname>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId, o.Id });
            builder.HasOne(o => o.User).WithMany(o => o.Nicknames).HasForeignKey(o => new { o.GuildId, o.UserId });
        });

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Guild> Guilds => Set<Guild>();
    public DbSet<GuildUser> GuildUsers => Set<GuildUser>();
    public DbSet<GuildChannel> Channels => Set<GuildChannel>();
    public DbSet<GuildUserChannel> UserChannels => Set<GuildUserChannel>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<SearchItem> SearchItems => Set<SearchItem>();
    public DbSet<Unverify> Unverifies => Set<Unverify>();
    public DbSet<UnverifyLog> UnverifyLogs => Set<UnverifyLog>();
    public DbSet<EmoteStatisticItem> Emotes => Set<EmoteStatisticItem>();
    public DbSet<RemindMessage> Reminders => Set<RemindMessage>();
    public DbSet<SelfunverifyKeepable> SelfunverifyKeepables => Set<SelfunverifyKeepable>();
    public DbSet<AutoReplyItem> AutoReplies => Set<AutoReplyItem>();
    public DbSet<EmoteSuggestion> EmoteSuggestions => Set<EmoteSuggestion>();
    public DbSet<ApiClient> ApiClients => Set<ApiClient>();
    public DbSet<Nickname> Nicknames => Set<Nickname>();
}
