using Discord;
using Discord.WebSocket;
using GrillBot.App.Handlers.Logging;
using GrillBot.App.Managers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Handlers.Logging;

[TestClass]
public class AuditLogLoggingHandlerTests : ServiceTest<AuditLogLoggingHandler>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;

    protected override AuditLogLoggingHandler CreateService()
    {
        var writer = new AuditLogWriteManager(DatabaseBuilder);
        return new AuditLogLoggingHandler(writer, Configuration);
    }

    [TestMethod]
    public async Task CanHandleAsync_NullException()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_Disabled()
    {
        var oldValue = Configuration["Discord:Logging:Enabled"];
        Configuration["Discord:Logging:Enabled"] = "false";

        try
        {
            var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
            Assert.IsFalse(result);
        }
        finally
        {
            Configuration["Discord:Logging:Enabled"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_InvalidSeverity()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Debug, "", new Exception());
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_GatewayReconnectException()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new GatewayReconnectException(""));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanHandleAsync_GatewayReconnectInnerException()
    {
        var result = await Service.CanHandleAsync(LogSeverity.Critical, "", new Exception("", new GatewayReconnectException("")));
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task InfoAsync()
    {
        await Service.InfoAsync("Test", "Test");
    }

    [TestMethod]
    public async Task WarningAsync()
    {
        await Service.WarningAsync("Test", "Test", new ArgumentException());
    }

    [TestMethod]
    public async Task ErrorAsync()
    {
        await Service.ErrorAsync("Test", "Test", new ArgumentException());
    }
}
