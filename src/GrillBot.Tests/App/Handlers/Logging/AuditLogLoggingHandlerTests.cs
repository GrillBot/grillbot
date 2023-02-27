using Discord;
using GrillBot.App.Handlers.Logging;
using GrillBot.App.Managers;
using GrillBot.Common.Exceptions;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Handlers.Logging;

[TestClass]
public class AuditLogLoggingHandlerTests : TestBase<AuditLogLoggingHandler>
{
    private static IConfiguration Configuration => TestServices.Configuration.Value;

    protected override AuditLogLoggingHandler CreateInstance()
    {
        var writer = new AuditLogWriteManager(DatabaseBuilder);
        return new AuditLogLoggingHandler(writer, Configuration);
    }

    [TestMethod]
    public async Task CanHandleAsync_NullException()
        => Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Critical, ""));

    [TestMethod]
    public async Task CanHandleAsync_Disabled()
    {
        var oldValue = Configuration["Discord:Logging:Enabled"];
        Configuration["Discord:Logging:Enabled"] = "false";

        try
        {
            Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Debug, "", new Exception()));
        }
        finally
        {
            Configuration["Discord:Logging:Enabled"] = oldValue;
        }
    }

    [TestMethod]
    public async Task CanHandleAsync_InvalidSeverity()
        => Assert.IsFalse(await Instance.CanHandleAsync(LogSeverity.Debug, "", new Exception()));

    [TestMethod]
    public async Task InfoAsync()
        => await Instance.InfoAsync("Test", "Test");

    [TestMethod]
    public async Task WarningAsync()
        => await Instance.WarningAsync("Test", "Test", new ArgumentException());

    [TestMethod]
    public async Task ErrorAsync()
        => await Instance.ErrorAsync("Test", "Test", new ArgumentException());

    [TestMethod]
    public async Task ErrorAsync_ApiException()
        => await Instance.ErrorAsync("API", "API", new ApiException("", new ArgumentException(), null, "", ""));
}
