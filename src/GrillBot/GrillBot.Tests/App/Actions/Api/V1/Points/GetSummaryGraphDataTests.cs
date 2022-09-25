using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class GetSummaryGraphDataTests : ApiActionTest<GetSummaryGraphData>
{
    protected override GetSummaryGraphData CreateAction()
    {
        return new GetSummaryGraphData(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task GetGraphDataAsync()
    {
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(new Database.Entity.PointsTransactionSummary
        {
            Day = DateTime.Now.Date,
            Guild = Database.Entity.Guild.FromDiscord(guild),
            GuildId = Consts.GuildId.ToString(),
            GuildUser = Database.Entity.GuildUser.FromDiscord(guild, user),
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetPointsSummaryParams();
        var result = await Action.ProcessAsync(filter);

        Assert.AreEqual(1, result.Count);
    }
}
