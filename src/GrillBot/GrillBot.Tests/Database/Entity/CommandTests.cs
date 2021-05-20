using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void HaveFlags_Single_True()
        {
            var command = new Command() { Flags = (int)CommandFlags.Blocked };

            Assert.IsTrue(command.HaveFlags(CommandFlags.Blocked));
        }

        [TestMethod]
        public void HaveFlags_Single_False()
        {
            var command = new Command();

            Assert.IsFalse(command.HaveFlags(CommandFlags.Blocked));
        }
    }
}
