using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Services.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class ExceptionFilterTests : ServiceTest<ExceptionFilter>
{
    protected override ExceptionFilter CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandService = DiscordHelper.CreateCommandsService();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingFactory = LoggingHelper.CreateLoggerFactory();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var loggingService = new LoggingService(discordClient, commandService, loggingFactory, configuration, DbFactory, interactions);

        return new ExceptionFilter(loggingService);
    }

    private static ExceptionContext CreateContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new(), new());

        return new ExceptionContext(actionContext, new List<IFilterMetadata>()) { Exception = exception };
    }

    [TestMethod]
    public async Task OperationCancelledException()
    {
        var context = CreateContext(new OperationCanceledException());

        await Service.OnExceptionAsync(context);
        Assert.IsTrue(context.ExceptionHandled);
    }

    [TestMethod]
    public async Task AnotherError()
    {
        var context = CreateContext(new ArgumentException("Test"));

        await Service.OnExceptionAsync(context);
        Assert.IsFalse(context.ExceptionHandled);
    }
}
