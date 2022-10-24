using System.Diagnostics.CodeAnalysis;
using System.IO;
using Discord;
using GrillBot.App.Services.User.Points;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
using GrillBot.Data.Exceptions;
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
        User = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetAvatar(Consts.AvatarId).Build();
        var userBuilder = new GuildUserBuilder().SetIdentity(User).SetAvatar(Consts.AvatarId);
        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUserAction(userBuilder.Build())
            .Build();
        GuildUser = userBuilder.SetGuild(Guild).Build();

        var texts = new TextsBuilder().Build();
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = TestServices.CounterManager.Value;
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var profilePicture = new ProfilePictureManager(CacheBuilder, counterManager);

        return new PointsService(discordClient, DatabaseBuilder, TestServices.Configuration.Value, messageCache, TestServices.Randomization.Value, profilePicture, texts);
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

    private async Task InitSummariesAsync()
    {
        await InitTransactionsAsync();
        Repository.ClearChangeTracker();

        foreach (var summary in Repository.Points.GetAllSummaries())
        {
            await Repository.AddAsync(new PointsTransactionSummary
            {
                Day = DateTime.Now.AddYears(-5).Date,
                GuildId = summary.GuildId,
                IsMerged = false,
                MessagePoints = summary.MessagePoints,
                ReactionPoints = summary.ReactionPoints,
                UserId = summary.UserId
            });

            await Repository.AddAsync(new PointsTransactionSummary
            {
                Day = DateTime.Now.AddYears(-3).Date,
                GuildId = summary.GuildId,
                IsMerged = false,
                MessagePoints = summary.MessagePoints,
                ReactionPoints = summary.ReactionPoints,
                UserId = summary.UserId
            });
        }

        await Repository.CommitAsync();
        Repository.ClearChangeTracker();
    }

    #region Recalculation

    [TestMethod]
    public async Task RecalculatePointsSummaryAsync_NoTransactions()
    {
        var result = await Service.RecalculatePointsSummaryAsync();

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task RecalculatePointsSummaryAsync_NothingToReport()
    {
        await Service.IncrementPointsAsync(GuildUser, 100);

        var result = await Service.RecalculatePointsSummaryAsync();

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task RecalculatePointsSummaryAsync_WithReaction()
    {
        await Service.IncrementPointsAsync(GuildUser, 100);
        await Repository.AddAsync(new PointsTransaction
        {
            Points = 1,
            AssingnedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            MessageId = Consts.MessageId.ToString(),
            ReactionId = Consts.PepeJamEmote,
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var result = await Service.RecalculatePointsSummaryAsync();
        Assert.IsNotNull(result);
    }

    #endregion

    #region Merge

    [TestMethod]
    public async Task MergeOldTransactionsAsync()
    {
        await InitTransactionsAsync();

        var result = await Service.MergeOldTransactionsAsync();
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.StartsWith("MergeTransactions"));
    }

    [TestMethod]
    public async Task MergeSummariesAsync()
    {
        await InitSummariesAsync();

        var result = await Service.MergeSummariesAsync();
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.StartsWith("MergeSummaries"));
    }

    #endregion

    #region Rendering

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task GetPointsOfUserImageAsync_NotFoundUser()
        => await Service.GetPointsOfUserImageAsync(Guild, GuildUser);

    [TestMethod]
    public async Task GetPointsOfUserImageAsync_Success()
    {
        await InitSummariesAsync();

        using var result = await Service.GetPointsOfUserImageAsync(Guild, User);
        Assert.IsTrue(File.Exists(result.Path));
    }

    #endregion
}
