using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetJobStatisticsTests : ApiActionTest<GetJobStatistics>
{
    protected override GetJobStatistics CreateAction()
    {
        return new GetJobStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.AuditLogItem
        {
            Data = JsonConvert.SerializeObject(new JobExecutionData
            {
                JobName = "Job"
            }, AuditLogWriter.SerializerSettings),
            Type = AuditLogItemType.JobCompleted
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
