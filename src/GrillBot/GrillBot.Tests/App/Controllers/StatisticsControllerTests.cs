using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class StatisticsControllerTests : ControllerTest<StatisticsController>
{
    protected override StatisticsController CreateController()
    {
        return new StatisticsController(DatabaseBuilder, CacheBuilder);
    }

    [TestMethod]
    public async Task GetDbStatusAsync()
    {
        var result = await Controller.GetDbStatusAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetDbCacheStatusAsync()
    {
        var result = await Controller.GetDbCacheStatusAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogsStatisticsByTypeAsync()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1,
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetAuditLogsStatisticsByTypeAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogStatisticsByDateAsync()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 1,
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetAuditLogsStatisticsByDateAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetCommandStatusAsync()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new CommandExecution { Command = "CMD" }),
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 2
        });
        await Repository.CommitAsync();

        var result = await Controller.GetTextCommandStatisticsAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetInteractionsStatusAsync()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new InteractionCommandExecuted()),
            Type = Database.Enums.AuditLogItemType.InteractionCommand,
            Id = 4
        });
        await Repository.CommitAsync();

        var result = await Controller.GetInteractionsStatusAsync();
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

        await Repository.AddAsync(new UnverifyLog
        {
            Operation = Database.Enums.UnverifyOperation.Update,
            FromUser = GuildUser.FromDiscord(guild, user),
            FromUserId = user.Id.ToString(),
            ToUser = GuildUser.FromDiscord(guild, user),
            ToUserId = user.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetUnverifyLogsStatisticsByOperationAsync();
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

        await Repository.AddAsync(new UnverifyLog
        {
            Operation = Database.Enums.UnverifyOperation.Update,
            FromUser = GuildUser.FromDiscord(guild, user),
            FromUserId = user.Id.ToString(),
            ToUser = GuildUser.FromDiscord(guild, user),
            ToUserId = user.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetUnverifyLogsStatisticsByDateAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetJobStatisticsAsync()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            Type = Database.Enums.AuditLogItemType.JobCompleted,
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetJobStatisticsAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetApiRequestsByDateAsync()
    {
        var result = await Controller.GetApiRequestsByDateAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetApiRequestsByEndpointAsync()
    {
        var now = DateTime.Now;

        await Repository.AddAsync(new AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new ApiRequest
            {
                StatusCode = "200 OK",
                Method = "GET",
                TemplatePath = "/test",
                EndAt = now.AddMinutes(1),
                StartAt = now
            }),
            Type = Database.Enums.AuditLogItemType.Api,
            Id = 5,
            CreatedAt = DateTime.Now
        });
        await Repository.CommitAsync();

        var result = await Controller.GetApiRequestsByEndpointAsync();
        CheckResult<OkObjectResult, List<StatisticItem>>(result);
    }

    [TestMethod]
    public async Task GetApiRequestsByStatusCodeAsync()
    {
        var now = DateTime.Now;

        await Repository.AddAsync(new AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new ApiRequest
            {
                StatusCode = "200 OK",
                Method = "GET",
                TemplatePath = "/test",
                EndAt = now.AddMinutes(1),
                StartAt = now
            }),
            Type = Database.Enums.AuditLogItemType.Api,
            Id = 5,
            CreatedAt = DateTime.Now
        });
        await Repository.CommitAsync();

        var result = await Controller.GetApiRequestsByStatusCodeAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }
}
