using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
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
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("A");
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));
            guild.Setup(o => o.Roles).Returns(new List<IRole>());

            using var context = CreateContext();

            context.Add(Guild.FromDiscord(guild.Object));
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildAsync(guild.Object).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("A");
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            using var context = CreateContext();

            context.InitGuildAsync(guild.Object).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_Initialized()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(Convert.ToUInt64(UserId));
            user.Setup(o => o.Username).Returns("U");

            using var context = CreateContext();

            context.Add(User.FromDiscord(user.Object));
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitUserAsync(user.Object).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_NonInitialized()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(Convert.ToUInt64(UserId));
            user.Setup(o => o.Username).Returns("U");

            using var context = CreateContext();

            context.InitUserAsync(user.Object).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildUserAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var user = new Mock<IGuildUser>();
            user.Setup(o => o.Id).Returns(Convert.ToUInt64(UserId));
            user.Setup(o => o.Nickname).Returns("GU");

            using var context = CreateContext();

            context.Add(GuildUser.FromDiscord(guild.Object, user.Object));
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildUserAsync(guild.Object, user.Object).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildUserAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var user = new Mock<IGuildUser>();
            user.Setup(o => o.Id).Returns(Convert.ToUInt64(UserId));
            user.Setup(o => o.Nickname).Returns("GU");

            using var context = CreateContext();

            context.InitGuildUserAsync(guild.Object, user.Object).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildChannelAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Id).Returns(Convert.ToUInt64(ChannelId));
            channel.Setup(o => o.Name).Returns("CH");

            using var context = CreateContext();

            context.Add(GuildChannel.FromDiscord(guild.Object, channel.Object, ChannelType.Category));
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.Category)
                .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildChannelAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Id).Returns(Convert.ToUInt64(ChannelId));
            channel.Setup(o => o.Name).Returns("CH");

            using var context = CreateContext();

            context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.Category)
                .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }
    }
}
