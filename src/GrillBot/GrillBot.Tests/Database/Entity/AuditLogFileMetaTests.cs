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
    }
}
