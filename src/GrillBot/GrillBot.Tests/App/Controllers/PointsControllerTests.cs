using System;
using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PointsControllerTests : ControllerTest<PointsController>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override bool CanInitProvider() => false;

    protected override PointsController CreateController()
    {
        var userBuilder = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator);

        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName)
            .SetGetUserAction(userBuilder.Build()).Build();
        User = userBuilder.SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new PointsApiService(DatabaseBuilder, mapper, client, ApiRequestContext);

        return new PointsController(apiService);
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
            IsReaction = false,
            MessageId = Consts.MessageId.ToString(),
            UserId = User.Id.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetPointTransactionsParams
        {
            Sort = { Descending = false }
        };
        var result = await AdminController.GetTransactionListAsync(filter);

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

        var result = await AdminController.GetTransactionListAsync(filter);
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
        var result = await AdminController.GetSummariesAsync(filter);
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
        var result = await AdminController.GetSummariesAsync(filter);
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
        var result = await AdminController.GetGraphDataAsync(filter);
        CheckResult<OkObjectResult, List<PointsSummaryBase>>(result);
    }

    [TestMethod]
    public async Task GetPointsLeaderboardAsync_WithoutData()
    {
        var result = await UserController.GetPointsLeaderboardAsync();
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

        var result = await UserController.GetPointsLeaderboardAsync();
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }
}