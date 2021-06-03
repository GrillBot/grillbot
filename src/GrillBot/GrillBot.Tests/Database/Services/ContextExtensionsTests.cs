using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.Database.Services
{
    [TestClass]
    public class ContextExtensionsTests
    {
        private const string GuildId = "2115465674";
        private const string UserId = "44897489486";
        private const string ChannelId = "45867615616";

        private static GrillBotContext CreateContext()
        {
            var opt = new DbContextOptionsBuilder()
                .UseInMemoryDatabase("GrillBot");

            return new TestingGrillBotContext(opt.Options);
        }

        [TestMethod]
        public void InitGuildAsync_Initialized()
        {
            using var context = CreateContext();

            context.Add(new Guild() { Id = GuildId });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildAsync(GuildId).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildAsync_NonInitialized()
        {
            using var context = CreateContext();

            context.InitGuildAsync(GuildId).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_Initialized()
        {
            using var context = CreateContext();

            context.Add(new User() { Id = UserId });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitUserAsync(UserId).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_NonInitialized()
        {
            using var context = CreateContext();

            context.InitUserAsync(UserId).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildUserAsync_Initialized()
        {
            using var context = CreateContext();

            context.Add(new GuildUser() { UserId = UserId, GuildId = GuildId });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildUserAsync(GuildId, UserId).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildUserAsync_NonInitialized()
        {
            using var context = CreateContext();

            context.InitGuildUserAsync(GuildId, UserId).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildChannelAsync_Initialized()
        {
            using var context = CreateContext();

            context.Add(new GuildChannel() { UserId = UserId, GuildId = GuildId, Id = ChannelId });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildChannelAsync(GuildId, ChannelId, UserId).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildChannelAsync_NonInitialized()
        {
            using var context = CreateContext();

            context.InitGuildChannelAsync(GuildId, ChannelId, UserId).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }
    }
}
