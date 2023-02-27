using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class GetPointsGraphDataTests : ApiActionTest<GetPointsGraphData>
{
    protected override GetPointsGraphData CreateInstance()
    {
        return new GetPointsGraphData(ApiRequestContext, DatabaseBuilder);
    }

    [TestMethod]
    public async Task GetGraphDataAsync()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new Database.Entity.PointsTransaction
        {
            AssingnedAt = DateTime.Now.Date,
            Guild = Database.Entity.Guild.FromDiscord(guild),
            GuildId = Consts.GuildId.ToString(),
            GuildUser = Database.Entity.GuildUser.FromDiscord(guild, user),
            Points = 100,
            MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            UserId = Consts.UserId.ToString(),
            ReactionId = ""
        });
        await Repository.CommitAsync();

        var filter = new GetPointTransactionsParams();
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(1, result.Count);
    }
}
