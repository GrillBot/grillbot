using Discord;
using GrillBot.Common.Managers.Logging.Handlers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.Common.Managers.Logging.Handlers;

[TestClass]
public class CommonLoggingHandlerTests : TestBase<CommonLoggerHandler>
{
    protected override CommonLoggerHandler CreateInstance()
    {
        return new CommonLoggerHandler(TestServices.LoggerFactory.Value);
    }

    [TestMethod]
    public async Task CanHandleAsync_API()
        => Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Info, "API"));

    [TestMethod]
    public async Task CanHandleAsync_Success()
        => Assert.IsTrue(await Instance.CanHandleAsync(LogSeverity.Info, "Gateway"));

    [TestMethod]
    public async Task InfoAsync()
        => await Instance.InfoAsync("Test", "Test");

    [TestMethod]
    public async Task WarningAsync_WithException()
        => await Instance.WarningAsync("test", "test", new Exception());

    [TestMethod]
    public async Task WarningAsync_WithoutException()
        => await Instance.WarningAsync("test", "test");

    [TestMethod]
    public async Task ErrorAsync()
        => await Instance.ErrorAsync("test", "test", new Exception());
}
