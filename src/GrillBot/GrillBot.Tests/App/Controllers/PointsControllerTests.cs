using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PointsControllerTests : ControllerTest<PointsController>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override PointsController CreateController()
    {
        var userBuilder = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator);

        Guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .SetGetUserAction(userBuilder.Build()).Build();
        User = userBuilder.SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        var apiService = new PointsApiService(DatabaseBuilder, TestServices.AutoMapper.Value, client, ApiRequestContext);
        return new PointsController(apiService, ApiRequestContext);
    }

    [TestMethod]
    public async Task GetTransactionListAsync_WithoutFilter()
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
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetPointTransactionsParams
        {
            Sort = { Descending = false }
        };
        var result = await Controller.GetTransactionListAsync(filter);

        CheckResult<OkObjectResult, PaginatedResponse<PointsTransaction>>(result);
    }

    [TestMethod]
    public async Task GetTransactionListAsync_WithFilter()
    {
        var filter = new GetPointTransactionsParams
        {
            AssignedAt = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Guild.Id.ToString(),
            OnlyMessages = true,
            OnlyReactions = true,
            UserId = User.Id.ToString()
        };

        var result = await Controller.GetTransactionListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<PointsTransaction>>(result);
    }

    [TestMethod]
    public async Task GetSummariesAsync_WithoutFilter()
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
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetPointsSummaryParams();
        var result = await Controller.GetSummariesAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<PointsSummary>>(result);
    }

    [TestMethod]
    public async Task GetSummariesAsync_WithFilter()
    {
        var filter = new GetPointsSummaryParams
        {
            Sort = { Descending = false },
            Days = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Guild.Id.ToString(),
            UserId = User.Id.ToString()
        };
        var result = await Controller.GetSummariesAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<PointsSummary>>(result);
    }

    [TestMethod]
    public async Task GetGraphDataAsync()
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
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetPointsSummaryParams();
        var result = await Controller.GetGraphDataAsync(filter);
        CheckResult<OkObjectResult, List<PointsSummaryBase>>(result);
    }

    [TestMethod]
    public async Task GetPointsLeaderboardAsync_WithoutData()
    {
        var result = await Controller.GetPointsLeaderboardAsync();
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }

    [TestMethod]
    public async Task GetPointsLeaderboardAsync_WithData()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(new Database.Entity.PointsTransactionSummary
        {
            Day = DateTime.Now,
            GuildId = Guild.Id.ToString(),
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            GuildUser = Database.Entity.GuildUser.FromDiscord(Guild, User),
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();

        var result = await Controller.GetPointsLeaderboardAsync();
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }

    [TestMethod]
    public async Task ComputeUserPointsAsync_WithoutUser()
    {
        var result = await Controller.ComputeUserPointsAsync(Consts.UserId);
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }

    [TestMethod]
    public async Task ComputeUserPointsAsync_WithUser()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.CommitAsync();

        var result = await Controller.ComputeUserPointsAsync(User.Id);
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }

    [TestMethod]
    public async Task ComputeLoggedUserPointsAsync()
    {
        var result = await Controller.ComputeLoggedUserPointsAsync();
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }
}
