using GrillBot.App.Controllers;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SystemControllerTests : ControllerTest<SystemController>
{
    protected override SystemController CreateController()
    {
        var environment = EnvironmentHelper.CreateEnv("Production");
        var client = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();
        var logger = LoggingHelper.CreateLogger<DiscordInitializationService>();
        var initialization = new DiscordInitializationService(logger);

        return new SystemController(environment, client, DbContext, initialization);
    }

    public override void Cleanup()
    {
        DbContext.AuditLogs.RemoveRange(DbContext.AuditLogs.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public void GetDiagnostics()
    {
        var result = Controller.GetDiagnostics();
        CheckResult<OkObjectResult, DiagnosticsInfo>(result);
    }

    [TestMethod]
    public async Task GetDbStatusAsync()
    {
        var result = await Controller.GetDbStatusAsync(CancellationToken.None);
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
        var result = await Controller.GetAuditLogsStatisticsAsync(CancellationToken.None);

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

        var result = await Controller.GetCommandStatusAsync("asdf");
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

        var result = await Controller.GetCommandStatusAsync();
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

        var result = await Controller.GetInteractionsStatusAsync("asdf");
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

        var result = await Controller.GetInteractionsStatusAsync();
        CheckResult<OkObjectResult, List<CommandStatisticItem>>(result);
    }

    [TestMethod]
    public void ChangeBotStatus()
    {
        var result = Controller.ChangeBotStatus(true);
        CheckResult<OkResult>(result);
    }
}
