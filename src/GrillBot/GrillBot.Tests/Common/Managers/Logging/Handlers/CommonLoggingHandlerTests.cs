using Discord;
using GrillBot.Common.Managers.Logging.Handlers;

namespace GrillBot.Tests.Common.Managers.Logging.Handlers;

[TestClass]
public class CommonLoggingHandlerTests : ServiceTest<CommonLoggerHandler>
{
    protected override CommonLoggerHandler CreateService()
    {
        var factory = LoggingHelper.CreateLoggerFactory();
        return new CommonLoggerHandler(factory);
    }

    [TestMethod]
    public async Task CanHandleAsync_API()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Info, "API");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_Success()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Info, "Gateway");
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task InfoAsync()
    {
        await Service.InfoAsync("Test", "Test");
    }

    [TestMethod]
    public async Task WarningAsync_WithException()
    {
        await Service.WarningAsync("test", "test", new Exception());
    }

    [TestMethod]
    public async Task WarningAsync_WithoutException()
    {
        await Service.WarningAsync("test", "test");
    }

    [TestMethod]
    public async Task ErrorAsync()
    {
        await Service.ErrorAsync("test", "test", new Exception());
    }
}
