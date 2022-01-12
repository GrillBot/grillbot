using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrillBot.Tests.Database.Services
{
    [TestClass]
    public class ContextExtensionsTests
    {
        private const string GuildId = "2115465674";
        private const string UserId = "44897489486";
        private const string ChannelId = "45867615616";
        private CancellationToken CancellationToken => CancellationToken.None;

        [TestMethod]
        public void InitGuildAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("A");
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));
            guild.Setup(o => o.Roles).Returns(new List<IRole>());

            var context = TestHelpers.CreateDbContext();

            try
            {
                context.Add(Guild.FromDiscord(guild.Object));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                context.InitGuildAsync(guild.Object, CancellationToken).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("A");
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var context = TestHelpers.CreateDbContext();
            try
            {
                context.InitGuildAsync(guild.Object, CancellationToken).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitUserAsync_Initialized()
        {
            var user = DiscordHelpers.CreateUserMock(Convert.ToUInt64(UserId), "U");
            var context = TestHelpers.CreateDbContext();

            try
            {
                context.Add(User.FromDiscord(user.Object));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                context.InitUserAsync(user.Object, CancellationToken).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitUserAsync_NonInitialized()
        {
            var user = DiscordHelpers.CreateUserMock(Convert.ToUInt64(UserId), "U");
            using var context = TestHelpers.CreateDbContext();

            try
            {
                context.InitUserAsync(user.Object, CancellationToken).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildUserAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var user = DiscordHelpers.CreateGuildUserMock(Convert.ToUInt64(UserId), "GU");
            var context = TestHelpers.CreateDbContext();

            try
            {
                context.Add(GuildUser.FromDiscord(guild.Object, user.Object));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                context.InitGuildUserAsync(guild.Object, user.Object, CancellationToken).ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildUserAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var user = DiscordHelpers.CreateGuildUserMock(Convert.ToUInt64(UserId), "GU");
            var context = TestHelpers.CreateDbContext();

            try
            {
                context.InitGuildUserAsync(guild.Object, user.Object, CancellationToken).ContinueWith(_ => Assert.IsTrue(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildChannelAsync_Initialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Id).Returns(Convert.ToUInt64(ChannelId));
            channel.Setup(o => o.Name).Returns("CH");

            var context = TestHelpers.CreateDbContext();
            try
            {
                context.Add(GuildChannel.FromDiscord(guild.Object, channel.Object, ChannelType.Category));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.Category, CancellationToken)
                    .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildChannelAsync_Thread()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var channel = new Mock<IThreadChannel>();
            channel.Setup(o => o.Id).Returns(Convert.ToUInt64(ChannelId));
            channel.Setup(o => o.Name).Returns("CH");
            channel.Setup(o => o.CategoryId).Returns(Convert.ToUInt64(ChannelId));

            var context = TestHelpers.CreateDbContext();
            context.Channels.RemoveRange(context.Channels);
            context.SaveChanges();

            try
            {
                context.Add(GuildChannel.FromDiscord(guild.Object, channel.Object, ChannelType.PublicThread));
                context.SaveChanges();
                context.ChangeTracker.Clear();

                context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.PublicThread, CancellationToken)
                    .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }

        [TestMethod]
        public void InitGuildChannelAsync_NonInitialized()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(Convert.ToUInt64(GuildId));

            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Id).Returns(Convert.ToUInt64(ChannelId));
            channel.Setup(o => o.Name).Returns("CH");

            var context = TestHelpers.CreateDbContext();
            try
            {
                context.InitGuildChannelAsync(guild.Object, channel.Object, ChannelType.Category, CancellationToken)
                .ContinueWith(_ => Assert.IsFalse(context.ChangeTracker.Entries().Any()));
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
