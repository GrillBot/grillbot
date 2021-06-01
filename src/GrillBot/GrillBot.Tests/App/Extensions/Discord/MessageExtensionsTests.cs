using Discord;
using GrillBot.App.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class MessageExtensionsTests
    {
        [TestMethod]
        public void IsCommand_Mention()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(370506820197810176);

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("<@370506820197810176> hello");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreNotEqual(0, argPos);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsCommand_NoLength()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Mention).Returns("<@1234>");

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreEqual(0, argPos);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsCommand_StringPrefix()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Mention).Returns("<@1234>");

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("$hello");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreNotEqual(0, argPos);
            Assert.IsTrue(result);
        }
    }
}
