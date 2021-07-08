using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class AuditEmoteInfoTests
    {
        [TestMethod]
        public void Constructor()
        {
            var emote = new AuditEmoteInfo(12345, "Emote");

            Assert.AreEqual("Emote", emote.Name);
            Assert.AreEqual((ulong)12345, emote.Id);
        }

        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.AreEqual((ulong)0, new AuditEmoteInfo().Id);
        }
    }
}
