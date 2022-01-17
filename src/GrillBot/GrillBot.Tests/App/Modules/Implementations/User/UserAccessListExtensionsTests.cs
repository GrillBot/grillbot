using Discord;
using GrillBot.Data.Modules.Implementations.User;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.User
{
    [TestClass]
    public class UserAccessListExtensionsTests
    {
        [TestMethod]
        public void WithUserAccessList()
        {
            var forUser = DiscordHelpers.CreateUserMock(12345, null);

            var user = DiscordHelpers.CreateGuildUserMock(0, "User");
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/image.png");

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(6789);

            var data = new List<Tuple<string, List<string>>>()
            {
                new Tuple<string, List<string>>("Category", new List<string>() { "Channel1", "Channel2" }),
                new Tuple<string, List<string>>("Category2", new List<string>() { "Channel3", "Channel4" })
            };

            var embed = new EmbedBuilder().WithUserAccessList(data, forUser.Object, user.Object, guild.Object, 0);

            Assert.IsNotNull(embed.Author);
            Assert.IsNotNull(embed.Footer);
            Assert.IsTrue(embed.Color.HasValue);
            Assert.AreNotEqual(Color.Default, embed.Color.Value);
            Assert.AreEqual(2, embed.Fields.Count);
        }
    }
}
