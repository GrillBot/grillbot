using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using Newtonsoft.Json;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class StatisticsControllerTests : ControllerTest<StatisticsController>
{
    protected override StatisticsController CreateController()
    {
        return new StatisticsController(DbFactory);
    }

    [TestMethod]
    public async Task GetDbStatusAsync()
    {
        var result = await AdminController.GetDbStatusAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogsStatisticsByTypeAsync()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAuditLogsStatisticsByTypeAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogStatisticsByDateAsync()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAuditLogsStatisticsByDateAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetCommandStatusAsync()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new CommandExecution() { Command = "CMD" }),
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 2
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetTextCommandStatisticsAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetInteractionsStatusAsync()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Data = JsonConvert.SerializeObject(new InteractionCommandExecuted()),
            Type = Database.Enums.AuditLogItemType.InteractionCommand,
            Id = 4
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetInteractionsStatusAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsStatisticsByOperationAsync()
    {
        var user = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();

        await DbContext.UnverifyLogs.AddAsync(new UnverifyLog()
        {
            Operation = Database.Enums.UnverifyOperation.Update,
            FromUser = GuildUser.FromDiscord(guild, user),
            FromUserId = user.Id.ToString(),
            ToUser = GuildUser.FromDiscord(guild, user),
            ToUserId = user.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetUnverifyLogsStatisticsByOperationAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsStatisticsByDateAsync()
    {
        var user = DataHelper.CreateGuildUser();
        var guild = DataHelper.CreateGuild();

        await DbContext.UnverifyLogs.AddAsync(new UnverifyLog()
        {
            Operation = Database.Enums.UnverifyOperation.Update,
            FromUser = GuildUser.FromDiscord(guild, user),
            FromUserId = user.Id.ToString(),
            ToUser = GuildUser.FromDiscord(guild, user),
            ToUserId = user.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetUnverifyLogsStatisticsByDateAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetJobStatisticsAsync()
    {
        await DbContext.AuditLogs.AddAsync(new AuditLogItem()
        {
            Type = Database.Enums.AuditLogItemType.JobCompleted,
            Data = "{}"
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetJobStatisticsAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }
}
