using Discord.WebSocket;
using GrillBot.Data.Controllers;
using GrillBot.Data.Services.AuditLog;
using GrillBot.Data.Services.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SystemControllerTests
{
    private static ServiceProvider CreateController(out SystemController controller)
    {
        var container = DIHelpers.CreateContainer();
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        var client = new DiscordSocketClient();
        var initializationService = new DiscordInitializationService(NullLogger<DiscordInitializationService>.Instance);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(o => o.EnvironmentName).Returns("Test");

        controller = new SystemController(envMock.Object, client, dbContext, initializationService);
        return container;
    }

    [TestMethod]
    public void GetDiagnostics()
    {
        using var _ = CreateController(out var controller);

        var result = controller.GetDiagnostics();
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetDbStatus()
    {
        using var _ = CreateController(out var controller);

        var result = controller.GetDbStatusAsync().Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetAuditLogsStatistics()
    {
        using var container = CreateController(out var controller);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
        dbContext.AuditLogs.Add(new GrillBot.Database.Entity.AuditLogItem()
        {
            Type = GrillBot.Database.Enums.AuditLogItemType.Command,
            Data = JsonConvert.SerializeObject(new CommandExecution(), AuditLogService.JsonSerializerSettings)
        });
        dbContext.SaveChanges();

        var result = controller.GetAuditLogsStatisticsAsync().Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetCommandStatus_WithSearch()
    {
        using var container = CreateController(out var controller);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
        dbContext.AuditLogs.Add(new GrillBot.Database.Entity.AuditLogItem()
        {
            Type = GrillBot.Database.Enums.AuditLogItemType.Command,
            Data = JsonConvert.SerializeObject(new CommandExecution() { Command = "Cmd" }, AuditLogService.JsonSerializerSettings)
        });
        dbContext.SaveChanges();

        var result = controller.GetCommandStatusAsync("Test").Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetCommandStatus_WithoutSearch()
    {
        using var container = CreateController(out var controller);

        var result = controller.GetCommandStatusAsync().Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetInteractionsStatus_WithSearch()
    {
        using var container = CreateController(out var controller);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
        dbContext.AuditLogs.Add(new GrillBot.Database.Entity.AuditLogItem()
        {
            Type = GrillBot.Database.Enums.AuditLogItemType.InteractionCommand,
            Data = JsonConvert.SerializeObject(new InteractionCommandExecuted() { Name = "Name", ModuleName = "Module", MethodName = "Method" }, AuditLogService.JsonSerializerSettings)
        });
        dbContext.SaveChanges();

        var result = controller.GetInteractionsStatusAsync("Test").Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void GetInteractionsStatus_WithoutSearch()
    {
        using var container = CreateController(out var controller);

        var result = controller.GetInteractionsStatusAsync().Result;
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public void ChangeBotStatus()
    {
        using var container = CreateController(out var controller);

        var result = controller.ChangeBotStatus(true);
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }
}
