using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class StatisticsControllerTests : ControllerTest<StatisticsController>
{
    protected override bool CanInitProvider() => false;

    protected override StatisticsController CreateController(IServiceProvider provider)
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
        var user = new GuildUserBuilder()
            .SetUsername(Consts.Username).SetId(Consts.UserId).SetDiscriminator(Consts.Discriminator)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

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
        var user = new GuildUserBuilder()
            .SetUsername(Consts.Username).SetId(Consts.UserId).SetDiscriminator(Consts.Discriminator)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

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
