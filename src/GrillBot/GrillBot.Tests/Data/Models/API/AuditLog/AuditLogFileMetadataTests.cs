using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.AuditLog
{
    [TestClass]
    public class AuditLogFileMetadataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            var metadata = new AuditLogFileMetadata();

            Assert.AreEqual(0, metadata.Id);
            Assert.IsNull(metadata.Filename);
            Assert.AreEqual(0, metadata.Size);
        }

        [TestMethod]
        public void BasicConstructor()
        {
            var entity = new AuditLogFileMeta()
            {
                Size = 50,
                Filename = "unknown.png",
                Id = 1
            };

            var meta = new AuditLogFileMetadata(entity);

            Assert.AreEqual(1, meta.Id);
            Assert.AreEqual("unknown.png", meta.Filename);
            Assert.AreEqual(50, meta.Size);
        }
    }
}
