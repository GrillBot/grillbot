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

        modelBuilder.Entity<EmoteStatisticItem>(builder =>
        {
            builder.HasKey(o => new { o.EmoteId, o.UserId, o.GuildId });
            builder.HasOne(o => o.User).WithMany(o => o.EmoteStatistics).HasForeignKey(o => new { o.GuildId, o.UserId });
            builder.HasOne(o => o.Guild).WithMany(o => o.EmoteStatistics);
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

        modelBuilder.Entity<AuditLogItem>(builder =>
        {
            builder.HasOne(o => o.Guild).WithMany(o => o.AuditLogs);
            builder.HasOne(o => o.ProcessedGuildUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ProcessedUserId });
            builder.HasOne(o => o.GuildChannel).WithMany().HasForeignKey(o => new { o.GuildId, o.ChannelId });
            builder.HasOne(o => o.ProcessedUser).WithMany().HasForeignKey(o => o.ProcessedUserId);
            builder.HasMany(o => o.Files).WithOne(o => o.AuditLogItem);
        });

        modelBuilder.Entity<SelfunverifyKeepable>(builder => builder.HasKey(o => new { o.GroupName, o.Name }));
        modelBuilder.Entity<ExplicitPermission>(builder => builder.HasKey(o => new { o.Command, o.TargetId }));

        modelBuilder.Entity<GuildEvent>(builder =>
        {
            builder.HasKey(o => new { o.Id, o.GuildId });
            builder.HasOne(o => o.Guild).WithMany(o => o.GuildEvents);
        });

        modelBuilder.Entity<EmoteSuggestion>(builder =>
        {
            builder.HasOne(o => o.Guild).WithMany();
            builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
        });

        modelBuilder.Entity<PointsTransaction>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId, o.MessageId, o.ReactionId });
            builder.HasOne(o => o.GuildUser).WithMany().HasForeignKey(o => new { o.GuildId, o.UserId });
        });

        modelBuilder.Entity<PointsTransactionSummary>(builder =>
        {
            builder.HasKey(o => new { o.GuildId, o.UserId, o.Day });
            builder.HasOne(o => o.GuildUser).WithMany().HasForeignKey(o => new { o.GuildId, o.UserId });
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
    public DbSet<AuditLogItem> AuditLogs => Set<AuditLogItem>();
    public DbSet<AuditLogFileMeta> AuditLogFiles => Set<AuditLogFileMeta>();
    public DbSet<EmoteStatisticItem> Emotes => Set<EmoteStatisticItem>();
    public DbSet<RemindMessage> Reminders => Set<RemindMessage>();
    public DbSet<SelfunverifyKeepable> SelfunverifyKeepables => Set<SelfunverifyKeepable>();
    public DbSet<ExplicitPermission> ExplicitPermissions => Set<ExplicitPermission>();
    public DbSet<AutoReplyItem> AutoReplies => Set<AutoReplyItem>();
    public DbSet<GuildEvent> GuildEvents => Set<GuildEvent>();
    public DbSet<EmoteSuggestion> EmoteSuggestions => Set<EmoteSuggestion>();
    public DbSet<PointsTransaction> PointsTransactions => Set<PointsTransaction>();
    public DbSet<PointsTransactionSummary> PointsTransactionSummaries => Set<PointsTransactionSummary>();
}
