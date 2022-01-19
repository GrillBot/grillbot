using GrillBot.App.Infrastructure.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GrillBot.Tests.App.Infrastructure.IO
{
    [TestClass]
    public class TemporaryFileTests
    {
        [TestMethod]
        public void Process()
        {
            using var file = new TemporaryFile("txt");

            File.WriteAllText(file.Path, "Test");
            Assert.IsFalse(string.IsNullOrEmpty(file.ToString()));
            Assert.AreEqual(".txt", Path.GetExtension(file.Path));
        }
    }
}
