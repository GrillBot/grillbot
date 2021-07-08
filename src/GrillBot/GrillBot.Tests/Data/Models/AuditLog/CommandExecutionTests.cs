using Discord.Commands;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class CommandExecutionTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            var command = new CommandExecution();

            Assert.IsNull(command.Command);
            Assert.IsNull(command.MessageContent);
            Assert.IsFalse(command.IsSuccess);
            Assert.IsNull(command.CommandError);
            Assert.IsNull(command.ErrorReason);
        }

        [TestMethod]
        public void Initializer()
        {
            _ = new CommandExecution()
            {
                ErrorReason = "Reason",
                CommandError = CommandError.BadArgCount,
                IsSuccess = true,
                Command = "Command",
                MessageContent = "Content"
            };

            Assert.IsTrue(true);
        }
    }
}
