using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class GetSummariesTests : ApiActionTest<GetSummaries>
{
    private IGuildUser User { get; set; }
    private IGuild Guild { get; set; }

    protected override GetSummaries CreateAction()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        return new GetSummaries(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await InitDataAsync(false);
        var filter = new GetPointsSummaryParams { Sort = { Descending = true, OrderBy = "MessagePoints" }, };
        var result = await Action.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
        Assert.IsNull(result.Data[0].MergeInfo);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetPointsSummaryParams
        {
            Sort = { Descending = false, OrderBy = "MessagePoints" },
            Days = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Guild.Id.ToString(),
            UserId = User.Id.ToString()
        };

        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_Merged()
    {
        await InitDataAsync(true);

        var filter = new GetPointsSummaryParams { Merged = true };
        var result = await Action.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
        Assert.IsNotNull(result.Data[0].MergeInfo);
    }

    private async Task InitDataAsync(bool merged)
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new Database.Entity.PointsTransactionSummary
        {
            Day = DateTime.Now.Date,
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            GuildId = Guild.Id.ToString(),
            GuildUser = Database.Entity.GuildUser.FromDiscord(Guild, User),
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = User.Id.ToString(),
            IsMerged = merged,
            MergedItemsCount = merged ? 50 : default,
            MergeRangeFrom = merged ? DateTime.Now : null,
            MergeRangeTo = merged ? DateTime.MaxValue : null
        });
        await Repository.CommitAsync();
    }
}
