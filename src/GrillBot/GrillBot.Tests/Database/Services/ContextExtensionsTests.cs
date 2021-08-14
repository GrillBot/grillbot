using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
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

        [TestMethod]
        public void InitGuildAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("A");
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));
            guild.Setup(o => o.Roles).Returns(new List<IRole>());

            using var context = TestHelpers.CreateDbContext();

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

            using var context = TestHelpers.CreateDbContext();

            context.InitGuildAsync(guild.Object).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_Initialized()
        {
            var user = DiscordHelpers.CreateUserMock(Convert.ToUInt64(UserId), "U");
            using var context = TestHelpers.CreateDbContext();

            context.Add(User.FromDiscord(user.Object));
            context.SaveChanges();
            context.ChangeTracker.Clear();

            context.InitUserAsync(user.Object).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitUserAsync_NonInitialized()
        {
            var user = DiscordHelpers.CreateUserMock(Convert.ToUInt64(UserId), "U");

            using var context = TestHelpers.CreateDbContext();

            context.InitUserAsync(user.Object).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
        }

        [TestMethod]
        public void InitGuildUserAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var user = DiscordHelpers.CreateGuildUserMock(Convert.ToUInt64(UserId), "GU");

            using var context = TestHelpers.CreateDbContext();

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

            var user = DiscordHelpers.CreateGuildUserMock(Convert.ToUInt64(UserId), "GU");

            using var context = TestHelpers.CreateDbContext();

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

            using var context = TestHelpers.CreateDbContext();

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

            using var context = TestHelpers.CreateDbContext();

            context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.Category)
                .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
        }
    }
}
