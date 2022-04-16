using GrillBot.Database.Entity;
using GrillBot.Database.Entity.Cache;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GrillBot.Database.Services
{
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
        public DbSet<MessageCacheIndex> MessageCacheIndexes { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }

        public IQueryable<TEntity> CreateQuery<TEntity>(IQueryableModel<TEntity> parameters, bool noTracking = false, bool splitQuery = false) where TEntity : class
        {
            var query = Set<TEntity>().AsQueryable();

            if (noTracking)
                query = query.AsNoTracking();
            if (splitQuery)
                query = query.AsSplitQuery();

            if (parameters != null)
            {
                query = parameters.SetIncludes(query);
                query = parameters.SetQuery(query);
                query = parameters.SetSort(query);
            }

            return query;
        }
    }
}
