using Discord;
using GrillBot.App.Services.User.Points;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User.Points;

[TestClass]
public class PointsServiceTests : ServiceTest<PointsService>
{
    private IGuild Guild { get; set; }
    private IGuildUser GuildUser { get; set; }
    private IUser User { get; set; }

    protected override PointsService CreateService()
    {
        User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetAvatar(Consts.AvatarId).Build();
        var userBuilder = new GuildUserBuilder(User).SetAvatar(Consts.AvatarId);
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUserAction(userBuilder.Build()).Build();
        GuildUser = userBuilder.SetGuild(Guild).Build();

        var texts = new TextsBuilder().Build();
        return new PointsService(DatabaseBuilder, TestServices.Configuration.Value, TestServices.Randomization.Value, texts);
    }

    private async Task InitDataAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(GuildUser);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(GuildUser);
        await Repository.CommitAsync();
    }

    private async Task InitTransactionsAsync()
    {
        await InitDataAsync();
        await Service.IncrementPointsAsync(GuildUser, 100);
        await Service.IncrementPointsAsync(GuildUser, 1100);

        await Repository.AddAsync(new PointsTransaction
        {
            Points = 10,
            AssingnedAt = DateTime.MinValue,
            GuildId = Consts.GuildId.ToString(),
            MessageId = Consts.MessageId.ToString(),
            ReactionId = Consts.PepeJamEmote,
            UserId = Consts.UserId.ToString()
        });

        await Repository.AddAsync(new PointsTransaction
        {
            Points = 100,
            AssingnedAt = DateTime.MinValue,
            GuildId = Consts.GuildId.ToString(),
            MessageId = Consts.MessageId.ToString(),
            ReactionId = Consts.OnlineEmoteId,
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();
        Repository.ClearChangeTracker();

        foreach (var transaction in Repository.Points.GetAll())
            transaction.AssingnedAt = DateTime.MinValue;
        await Repository.CommitAsync();
    }

    #region Merge

    [TestMethod]
    public async Task MergeOldTransactionsAsync()
    {
        await InitTransactionsAsync();

        var result = await Service.MergeOldTransactionsAsync();
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.StartsWith("MergeTransactions"));
    }

    #endregion
}
