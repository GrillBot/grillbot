using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class AuditLogFileMetaTests
    {
        [TestMethod]
        public void Extension()
        {
            var metadata = new AuditLogFileMeta() { Filename = "Image.jpg" };

            Assert.AreEqual(".jpg", metadata.Extension);
        }

        [TestMethod]
        public void FilenameWithoutExtension()
        {
            var metadata = new AuditLogFileMeta() { Filename = "Image.jpg" };

            Assert.AreEqual("Image", metadata.FilenameWithoutExtension);
        }

        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new AuditLogFileMeta());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var meta = new AuditLogFileMeta()
            {
                AuditLogItem = new(),
                AuditLogItemId = 12345,
                Filename = "File",
                Id = 12345,
                Size = 13156486
            };

            TestHelpers.CheckNonDefaultPropertyValues(meta);
        }
    }
}
