using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetApiStatisticsTests : ApiActionTest<GetApiStatistics>
{
    protected override GetApiStatistics CreateAction()
    {
        return new GetApiStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddCollectionAsync(new[]
        {
            CreateLogItem(new ApiRequest
            {
                Language = "cs",
                Method = "Method",
                Path = "/route",
                ActionName = "Action",
                ControllerName = "Controller",
                EndAt = DateTime.Now.AddDays(1),
                StatusCode = "200 (OK)",
                TemplatePath = "/route",
                LoggedUserRole = "Admin",
                StartAt = DateTime.Now
            }),
            CreateLogItem(new ApiRequest())
        });
        await Repository.CommitAsync();
    }

    private static Database.Entity.AuditLogItem CreateLogItem(ApiRequest request)
    {
        return new Database.Entity.AuditLogItem
        {
            Type = AuditLogItemType.Api,
            CreatedAt = DateTime.Now,
            ProcessedUserId = Consts.UserId.ToString(),
            Data = JsonConvert.SerializeObject(request, AuditLogWriter.SerializerSettings)
        };
    }

    [TestMethod]
    public async Task ProcessByDateAsync()
    {
        await InitDataAsync();
        var result = await Action.ProcessByDateAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessByEndpointAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByEndpointAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessByStatusCodeAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByStatusCodeAsync();
        Assert.AreEqual(1, result.Count);
    }
}
