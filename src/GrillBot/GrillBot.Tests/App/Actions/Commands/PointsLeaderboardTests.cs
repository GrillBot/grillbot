using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class PointsLeaderboardTests : CommandActionTest<PointsLeaderboard>
{
    protected override IGuild Guild { get; }
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuildUser User { get; }
        = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(new GuildBuilder(Consts.GuildId, Consts.GuildName).Build()).Build();

    protected override PointsLeaderboard CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("Points/Board/Counts/FiveAndMore", "en-US", "{0} points")
            .AddText("Points/Board/Row", "en-US", "**{0:N0}.** {1} ({2})")
            .AddText("Points/Board/Title", "en-US", "Title")
            .Build();
        var formatHelper = new FormatHelper(texts);

        var action = new PointsLeaderboard(DatabaseBuilder, texts, formatHelper);
        action.Init(Context);

        return action;
    }

    private async Task InitDataAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(User);

        await Repository.AddAsync(new PointsTransactionSummary
        {
            Day = DateTime.Now.Date,
            GuildId = Consts.GuildId.ToString(),
            IsMerged = false,
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NoActivity()
        => await Action.ProcessAsync(0);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        var result = await Action.ProcessAsync(0);

        Assert.IsNotNull(result.embed);
        Assert.IsNull(result.paginationComponent);
    }
}
