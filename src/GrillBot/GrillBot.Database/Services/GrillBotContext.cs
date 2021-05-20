using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services
{
    public class GrillBotContext : DbContext
    {
        public GrillBotContext(DbContextOptions options) : base(options) { }

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
            });

            modelBuilder.Entity<User>(builder => builder.HasIndex(o => o.ApiToken).IsUnique());
            modelBuilder.Entity<EmoteStatisticItem>(builder => builder.HasOne(o => o.User).WithMany(o => o.UsedEmotes));

            modelBuilder.Entity<RemindMessage>(buider =>
            {
                buider.HasOne(o => o.FromUser).WithMany(o => o.OutgoingReminders);
                buider.HasOne(o => o.ToUser).WithMany(o => o.IncomingReminders);
            });

            modelBuilder.Entity<GuildChannel>(builder =>
            {
                builder.HasKey(o => new { o.GuildId, o.Id });
                builder.HasOne(o => o.Guild).WithMany(o => o.Channels);
                builder.HasOne(o => o.User).WithMany(o => o.Channels);
            });

            modelBuilder.Entity<SearchItem>(builder =>
            {
                builder.HasOne(o => o.User).WithMany(o => o.SearchItems);
                builder.HasOne(o => o.Channel).WithMany(o => o.SearchItems).HasForeignKey(o => new { o.GuildId, o.ChannelId });
            });

            modelBuilder.Entity<Unverify>(builder =>
            {
                builder.HasKey(o => new { o.GuildId, o.UserId });
                builder.HasOne(o => o.GuildUser).WithOne(o => o.Unverify).HasForeignKey<Unverify>(o => new { o.GuildId, o.UserId });
                builder.HasOne(o => o.UnverifyLog).WithOne(o => o.Unverify);
            });

            modelBuilder.Entity<UnverifyLog>(builder =>
            {
                builder.HasOne(o => o.FromUser).WithMany().HasForeignKey(o => new { o.GuildId, o.FromUserId });
                builder.HasOne(o => o.ToUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ToUserId });
            });

            modelBuilder.Entity<AuditLogItem>(builder =>
            {
                builder.HasOne(o => o.ProcessedGuildUser).WithMany().HasForeignKey(o => new { o.GuildId, o.ProcessedUserId });
                builder.HasOne(o => o.GuildChannel).WithMany().HasForeignKey(o => new { o.GuildId, o.ChannelId });
                builder.HasMany(o => o.Files).WithOne(o => o.AuditLogItem);
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildUser> GuildUsers { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<SearchItem> SearchItems { get; set; }
        public DbSet<Unverify> Unverifies { get; set; }
        public DbSet<UnverifyLog> UnverifyLogs { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<AuditLogItem> AuditLogs { get; set; }
        public DbSet<AuditLogFileMeta> AuditLogFiles { get; set; }
    }
}
