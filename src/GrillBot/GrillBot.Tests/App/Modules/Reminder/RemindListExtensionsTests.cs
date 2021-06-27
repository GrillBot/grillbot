using Discord;
using Discord.WebSocket;
using GrillBot.App.Modules.Reminder;
using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Reminder
{
    [TestClass]
    public class RemindListExtensionsTests
    {
        [TestMethod]
        public void WithRemindList_Empty()
        {
            var data = new List<RemindMessage>();
            var client = new DiscordSocketClient();

            var forUser = new Mock<IUser>();
            forUser.Setup(o => o.Id).Returns(1234);
            forUser.Setup(o => o.Username).Returns("Test");
            forUser.Setup(o => o.Discriminator).Returns("0000");
            forUser.Setup(o => o.AvatarId).Returns((string)null);
            forUser.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/image.png");

            var embed = (new EmbedBuilder()
                .WithRemindListAsync(data, client, forUser.Object, forUser.Object, 0)).Result;

            Assert.AreEqual(0, embed.Fields.Count);
        }
    }
}
