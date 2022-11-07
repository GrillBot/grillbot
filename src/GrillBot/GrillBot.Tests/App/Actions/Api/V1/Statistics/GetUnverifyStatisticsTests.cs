using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetUnverifyStatisticsTests : ApiActionTest<GetUnverifyStatistics>
{
    protected override GetUnverifyStatistics CreateAction()
    {
        return new GetUnverifyStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, user));
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            Operation = UnverifyOperation.Update,
            FromUserId = user.Id.ToString(),
            ToUserId = user.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Data = "{}"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessByOperationAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByOperationAsync();
        Assert.AreEqual(Enum.GetValues<UnverifyOperation>().Length, result.Count);
    }

    [TestMethod]
    public async Task ProcessByDateAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessByDateAsync();
        Assert.AreEqual(1, result.Count);
    }
}
