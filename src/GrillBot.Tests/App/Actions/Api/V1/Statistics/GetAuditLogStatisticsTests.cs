using System.Linq;
using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetAuditLogStatisticsTests : ApiActionTest<GetAuditLogStatistics>
{
    protected override GetAuditLogStatistics CreateAction()
    {
        return new GetAuditLogStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.AuditLogItem
        {
            Data = "This is test",
            Type = AuditLogItemType.Info,
            CreatedAt = DateTime.Now
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessByTypeAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByTypeAsync();
        Assert.AreEqual(Enum.GetValues<AuditLogItemType>().Count(o => o != AuditLogItemType.None), result.Count);
    }

    [TestMethod]
    public async Task ProcessByDateAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByDateAsync();
        Assert.AreEqual(1, result.Count);
    }
}
