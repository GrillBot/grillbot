using System.Linq;
using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetAuditLogStatisticsTests : ApiActionTest<GetAuditLogStatistics>
{
    protected override GetAuditLogStatistics CreateInstance()
    {
        return new GetAuditLogStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var itemWithFiles = new AuditLogItem
        {
            CreatedAt = DateTime.Now,
            Data = "This is test",
            Type = AuditLogItemType.Info,
        };
        itemWithFiles.Files.Add(new AuditLogFileMeta
        {
            Filename = "Filename.png",
            Size = 1500
        });

        await Repository.AddCollectionAsync(new[]
        {
            new AuditLogItem
            {
                Data = "This is test",
                Type = AuditLogItemType.Info,
                CreatedAt = DateTime.Now
            },
            itemWithFiles
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ByType);
        Assert.IsNotNull(result.ByDate);
        Assert.IsNotNull(result.FileCounts);
        Assert.IsNotNull(result.FileSizes);
        Assert.AreEqual(Enum.GetValues<AuditLogItemType>().Count(o => o != AuditLogItemType.None), result.ByType.Count);
        Assert.AreEqual(1, result.ByDate.Count);
        Assert.AreEqual(1, result.FileCounts.Count);
        Assert.AreEqual(1, result.FileSizes.Count);
    }
}
