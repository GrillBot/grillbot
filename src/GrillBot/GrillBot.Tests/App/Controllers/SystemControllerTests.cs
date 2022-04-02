using GrillBot.App.Controllers;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SystemControllerTests : ControllerTest<SystemController>
{
    protected override SystemController CreateController()
    {
        var environment = EnvironmentHelper.CreateEnv("Production");
        var client = DiscordHelper.CreateClient();
        var logger = LoggingHelper.CreateLogger<DiscordInitializationService>();
        var initialization = new DiscordInitializationService(logger);

        return new SystemController(environment, client, DbContext, initialization);
    }

    [TestMethod]
    public void GetDiagnostics()
    {
        var result = AdminController.GetDiagnostics();
        CheckResult<OkObjectResult, DiagnosticsInfo>(result);
    }

    [TestMethod]
    public async Task GetDbStatusAsync()
    {
        var result = await AdminController.GetDbStatusAsync(CancellationToken.None);
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogsStatisticsAsync()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1
        });
        await DbContext.SaveChangesAsync();
        var result = await AdminController.GetAuditLogsStatisticsAsync(CancellationToken.None);

        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetCommandStatusAsync_WithSearch()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new CommandExecution() { Command = "CMD" }),
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetCommandStatusAsync("asdf");
        CheckResult<OkObjectResult, List<CommandStatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetCommandStatusAsync_WithoutSearch()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new CommandExecution() { Command = "CMD" }),
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 2
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetCommandStatusAsync();
        CheckResult<OkObjectResult, List<CommandStatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetInteractionsStatusAsync_WithSearch()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new InteractionCommandExecuted()),
            Type = Database.Enums.AuditLogItemType.InteractionCommand,
            Id = 3
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetInteractionsStatusAsync("asdf");
        CheckResult<OkObjectResult, List<CommandStatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetInteractionsStatusAsync_WithoutSearch()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new InteractionCommandExecuted()),
            Type = Database.Enums.AuditLogItemType.InteractionCommand,
            Id = 4
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetInteractionsStatusAsync();
        CheckResult<OkObjectResult, List<CommandStatisticItem>>(result);
    }

    [TestMethod]
    public void ChangeBotStatus()
    {
        var result = AdminController.ChangeBotStatus(true);
        CheckResult<OkResult>(result);
    }
}
