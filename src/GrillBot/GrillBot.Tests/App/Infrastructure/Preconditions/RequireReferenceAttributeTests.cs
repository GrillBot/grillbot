using Discord;
using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.App.Infrastructure.Preconditions
{
    [TestClass]
    public class RequireReferenceAttributeTests
    {
        [TestMethod]
        public void CheckPermissions_True()
        {
            var message = new Mock<IUserMessage>();
            message.Setup(o => o.Reference).Returns(new MessageReference());

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Message).Returns(message.Object);

            var attribute = new RequireReferenceAttribute();
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsTrue(result.IsSuccess);
        }

        [TestMethod]
        public void CheckPermissions_False()
        {
            var message = new Mock<IUserMessage>();
            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Message).Returns(message.Object);

            var attribute = new RequireReferenceAttribute();
            var result = attribute.CheckPermissionsAsync(context.Object, null, null).Result;

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
