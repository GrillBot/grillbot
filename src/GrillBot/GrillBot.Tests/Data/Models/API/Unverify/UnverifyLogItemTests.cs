using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogItem());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var operations = new[]
            {
                UnverifyOperation.Autoremove,
                UnverifyOperation.Recover,
                UnverifyOperation.Remove,
                UnverifyOperation.Selfunverify,
                UnverifyOperation.Unverify,
                UnverifyOperation.Update
            };

            foreach (var operation in operations)
            {
                var entity = new GrillBot.Database.Entity.UnverifyLog()
                {
                    Guild = new(),
                    FromUser = new() { User = new() },
                    ToUser = new() { User = new() },
                    Data = "{}",
                    Operation = operation
                };

                var logItem = new UnverifyLogItem(entity);

                Assert.IsNotNull(logItem);
            }
        }
    }
}
