using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands.Points;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Points;

[TestClass]
public class PointsLeaderboardTests : CommandActionTest<PointsLeaderboard>
{
    protected override IGuild Guild { get; }
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuildUser User { get; }
        = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(new GuildBuilder(Consts.GuildId, Consts.GuildName).Build()).Build();

    protected override PointsLeaderboard CreateInstance()
    {
        var texts = TestServices.Texts.Value;
        var formatHelper = new FormatHelper(texts);

        return InitAction(new PointsLeaderboard(DatabaseBuilder, texts, formatHelper));
    }

    private async Task InitDataAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(User);

        await Repository.AddAsync(new PointsTransaction
        {
            AssingnedAt = DateTime.Now.Date,
            GuildId = Consts.GuildId.ToString(),
            ReactionId = "",
            Points = 50,
            MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NoActivity()
        => await Instance.ProcessAsync(0);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        var result = await Instance.ProcessAsync(0);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
    }
}
