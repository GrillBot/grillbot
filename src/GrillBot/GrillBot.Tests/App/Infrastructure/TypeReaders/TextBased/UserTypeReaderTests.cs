using Discord;
using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders.TextBased;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders.TextBased
{
    [TestClass]
    public class UserTypeReaderTests
    {
        [TestMethod]
        public void Read_Me()
        {
            var user = DiscordHelpers.CreateUserMock(12345, "");
            var context = new Mock<ICommandContext>();
            context.Setup(o => o.User).Returns(user.Object);

            var reader = new UserTypeReader();
            var result = reader.ReadAsync(context.Object, "me", null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual((ulong)12345, ((IUser)result.Values.First().Value).Id);
        }
    }
}
