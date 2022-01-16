using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.AuditLog;

[TestClass]
public class AuditThreadInfoTests
{
    [TestMethod]
    public void Constructor_Channel()
    {
        var thread = new Mock<IThreadChannel>();
        thread.Setup(o => o.Id).Returns(12345);

        var threadInfo = new AuditThreadInfo(thread.Object);

        Assert.IsNotNull(threadInfo);
        Assert.AreEqual((ulong)12345, threadInfo.Id);
    }

    [TestMethod]
    public void EmptyConstructor()
    {
        TestHelpers.CheckDefaultPropertyValues(new AuditThreadInfo());
    }
}
