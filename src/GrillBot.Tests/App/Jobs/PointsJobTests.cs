using Discord;
using GrillBot.App.Jobs;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class PointsJobTests : JobTest<PointsJob>
{
    private IGuild Guild { get; set; } = null!;
    private IGuildUser GuildUser { get; set; } = null!;
    private IUser User { get; set; } = null!;

    protected override PointsJob CreateInstance()
    {
        User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetAvatar(Consts.AvatarId).Build();
        var userBuilder = new GuildUserBuilder(User).SetAvatar(Consts.AvatarId);
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUsersAction(new[] { userBuilder.Build() }).Build();
        GuildUser = userBuilder.SetGuild(Guild).Build();

        return new PointsJob(DatabaseBuilder, TestServices.Provider.Value);
    }

    private async Task InitTransactionsAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(GuildUser);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(GuildUser);

        await Repository.AddCollectionAsync(new[]
        {
            new PointsTransaction
            {
                Points = 10,
                AssingnedAt = DateTime.MinValue,
                GuildId = Consts.GuildId.ToString(),
                MessageId = Consts.MessageId.ToString(),
                ReactionId = Consts.PepeJamEmote,
                UserId = Consts.UserId.ToString()
            },
            new PointsTransaction
            {
                Points = 100,
                AssingnedAt = DateTime.MinValue,
                GuildId = Consts.GuildId.ToString(),
                MessageId = Consts.MessageId.ToString(),
                ReactionId = Consts.OnlineEmoteId,
                UserId = Consts.UserId.ToString()
            }
        });

        await Repository.CommitAsync();
        Repository.ClearChangeTracker();

        foreach (var transaction in Repository.Points.GetAll())
            transaction.AssingnedAt = DateTime.MinValue;
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task Execute_Success()
    {
        await InitTransactionsAsync();

        await Execute(context =>
        {
            Assert.IsFalse(string.IsNullOrEmpty(context.Result as string));
            Assert.IsTrue(context.Result.ToString()!.StartsWith("MergeTransactions"));
        });
    }

    [TestMethod]
    public async Task Execute_NoTransactions()
        => await Execute(context => Assert.IsTrue(string.IsNullOrEmpty(context.Result as string)));
}
