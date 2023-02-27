using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class GetTransactionListTests : ApiActionTest<GetTransactionList>
{
    private IGuildUser User { get; set; }
    private IGuild Guild { get; set; }

    protected override GetTransactionList CreateInstance()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        return new GetTransactionList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await InitDataAsync(false);
        var filter = new GetPointTransactionsParams
        {
            Sort = { Descending = false }
        };

        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetPointTransactionsParams
        {
            AssignedAt = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Guild.Id.ToString(),
            OnlyMessages = true,
            OnlyReactions = true,
            UserId = User.Id.ToString()
        };

        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_MergedItems()
    {
        await InitDataAsync(true);

        var filter = new GetPointTransactionsParams { Merged = true };
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
        Assert.IsNotNull(result.Data[0].MergeInfo);
    }

    private async Task InitDataAsync(bool merged)
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new Database.Entity.PointsTransaction
        {
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            Points = 50,
            AssingnedAt = DateTime.Now,
            GuildId = Guild.Id.ToString(),
            GuildUser = Database.Entity.GuildUser.FromDiscord(Guild, User),
            ReactionId = "",
            MessageId = Consts.MessageId.ToString(),
            UserId = User.Id.ToString(),
            MergedItemsCount = merged ? 1 : 0,
            MergeRangeFrom = merged ? DateTime.Now : null,
            MergeRangeTo = merged ? DateTime.MaxValue : null
        });
        await Repository.CommitAsync();
    }
}
