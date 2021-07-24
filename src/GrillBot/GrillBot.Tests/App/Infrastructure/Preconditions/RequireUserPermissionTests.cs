using Discord;
using GrillBot.App.Infrastructure.Preconditions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.Preconditions
{
    [TestClass]
    public class RequireUserPermissionTests
    {
        [TestMethod]
        public void InvalidContext()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);

            var attribute = new RequireUserPermissionAttribute(new[] { GuildPermission.Administrator }, false);
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void NoContext()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<IDMChannel>().Object);

            var attribute = new RequireUserPermissionAttribute(new[] { ChannelPermission.AddReactions }, false);
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void InvalidContext_Guild()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<ITextChannel>().Object);
            context.Setup(o => o.Guild).Returns(new Mock<IGuild>().Object);

            var attribute = new RequireUserPermissionAttribute(new[] { ChannelPermission.AddReactions }, false) { Contexts = Discord.Commands.ContextType.DM };
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void GuildPermissions_Invalid()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<ITextChannel>().Object);
            context.Setup(o => o.Guild).Returns(new Mock<IGuild>().Object);
            context.Setup(o => o.User).Returns(new Mock<IGuildUser>().Object);

            var perms = Enum.GetValues<GuildPermission>().ToArray();
            var attribute = new RequireUserPermissionAttribute(perms, false, true);
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ChannelPermissions_Invalid()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<ITextChannel>().Object);
            context.Setup(o => o.Guild).Returns(new Mock<IGuild>().Object);
            context.Setup(o => o.User).Returns(new Mock<IGuildUser>().Object);

            var perms = Enum.GetValues<ChannelPermission>().ToArray();
            var attribute = new RequireUserPermissionAttribute(perms, false, true);
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void NotSocketGuildUser()
        {
            var context = new Mock<Discord.Commands.ICommandContext>();
            context.Setup(o => o.Channel).Returns(new Mock<ITextChannel>().Object);
            context.Setup(o => o.Guild).Returns(new Mock<IGuild>().Object);
            context.Setup(o => o.User).Returns(new Mock<IGuildUser>().Object);

            var perms = Enum.GetValues<GuildPermission>().ToArray();
            var attribute = new RequireUserPermissionAttribute(perms, true, true);
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
