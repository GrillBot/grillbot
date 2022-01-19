using Discord.Commands;
using GrillBot.App.Infrastructure.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Infrastructure.Commands
{
    [TestClass]
    public class CommandRedirectResultTests
    {
        [TestMethod]
        public void Constructor()
        {
            var result = new CommandRedirectResult("angry @user");

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("angry @user", result.NewCommand);
            Assert.AreEqual(CommandError.Unsuccessful, result.Error);
        }
    }
}
