using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetAvgTimesTests : ApiActionTest<GetAvgTimes>
{
    protected override GetAvgTimes CreateAction()
    {
        return new GetAvgTimes(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddCollectionAsync(new[]
        {
            CreateLogItem(new InteractionCommandExecuted { Name = "Inter", ModuleName = "Module", MethodName = "Method", Duration = 50 }, AuditLogItemType.InteractionCommand),
            CreateLogItem(new JobExecutionData { EndAt = DateTime.Now.AddHours(1), StartAt = DateTime.Now }, AuditLogItemType.JobCompleted),
            CreateLogItem(new ApiRequest { EndAt = DateTime.Now.AddDays(1), StartAt = DateTime.Now, ApiGroupName = "V1", StatusCode = "200 OK", Method = "GET", TemplatePath = "/" },
                AuditLogItemType.Api),
            CreateLogItem(new ApiRequest { EndAt = DateTime.Now.AddDays(1), StartAt = DateTime.Now, ApiGroupName = "V2", StatusCode = "200 OK", Method = "GET", TemplatePath = "/" },
                AuditLogItemType.Api),
            CreateLogItem(new ApiRequest(), AuditLogItemType.Api)
        });
        await Repository.CommitAsync();
    }

    private static Database.Entity.AuditLogItem CreateLogItem(object data, AuditLogItemType type)
    {
        return new Database.Entity.AuditLogItem
        {
            Data = JsonConvert.SerializeObject(data, AuditLogWriter.SerializerSettings),
            Type = type,
            CreatedAt = DateTime.Now,
            ProcessedUserId = Consts.UserId.ToString()
        };
    }

    [TestMethod]
    public async Task ProcessAsync_NoData()
    {
        var result = await Action.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Interactions.Count);
        Assert.AreEqual(0, result.Jobs.Count);
        Assert.AreEqual(0, result.ExternalApi.Count);
        Assert.AreEqual(0, result.InternalApi.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_WithData()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Interactions.Count);
        Assert.AreEqual(1, result.Jobs.Count);
        Assert.AreEqual(1, result.ExternalApi.Count);
        Assert.AreEqual(1, result.InternalApi.Count);
    }
}
