using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;

namespace GrillBot.Database.Services
{
    public class GrillBotContext : DbContext
    {
        public GrillBotContext(DbContextOptions options) : base(options)
        {
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
                builder.HasKey(o => new { o.EmoteId, o.UserId });
                builder.HasOne(o => o.User).WithMany(o => o.UsedEmotes);
            });

            modelBuilder.Entity<RemindMessage>(buider =>
            {
                buider.HasOne(o => o.FromUser).WithMany(o => o.OutgoingReminders);
                buider.HasOne(o => o.ToUser).WithMany(o => o.IncomingReminders);
            });

            modelBuilder.Entity<GuildUserChannel>(builder =>
            {
                builder.HasKey(o => new { o.GuildId, o.Id, o.UserId });
                builder.HasOne(o => o.Guild).WithMany().HasForeignKey(o => o.GuildId);
                builder.HasOne(o => o.User).WithMany(o => o.Channels).HasForeignKey(o => new { o.GuildId, o.UserId });
                builder.HasOne(o => o.Channel).WithMany(o => o.Channels).HasForeignKey(o => new { o.GuildId, o.Id });
            });

            modelBuilder.Entity<GuildChannel>(builder =>
            {
                builder.HasKey(o => new { o.GuildId, o.ChannelId });
                builder.HasOne(o => o.Guild).WithMany(o => o.Channels);
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
                builder.HasMany(o => o.Files).WithOne(o => o.AuditLogItem);
            });

            modelBuilder.Entity<SelfunverifyKeepable>(builder => builder.HasKey(o => new { o.GroupName, o.Name }));
            modelBuilder.Entity<ExplicitPermission>(builder => builder.HasKey(o => new { o.Command, o.TargetId }));

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildUser> GuildUsers { get; set; }
        public DbSet<GuildChannel> Channels { get; set; }
        public DbSet<GuildUserChannel> UserChannels { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<SearchItem> SearchItems { get; set; }
        public DbSet<Unverify> Unverifies { get; set; }
        public DbSet<UnverifyLog> UnverifyLogs { get; set; }
        public DbSet<AuditLogItem> AuditLogs { get; set; }
        public DbSet<AuditLogFileMeta> AuditLogFiles { get; set; }
        public DbSet<EmoteStatisticItem> Emotes { get; set; }
        public DbSet<RemindMessage> Reminders { get; set; }
        public DbSet<SelfunverifyKeepable> SelfunverifyKeepables { get; set; }
        public DbSet<ExplicitPermission> ExplicitPermissions { get; set; }
        public DbSet<AutoReplyItem> AutoReplies { get; set; }
    }
}
