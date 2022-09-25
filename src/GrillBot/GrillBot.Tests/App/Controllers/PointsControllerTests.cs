using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Models.API.Users;
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

        var apiService = new PointsApiService(DatabaseBuilder, TestServices.AutoMapper.Value, client);
        return new PointsController(apiService, ApiRequestContext, ServiceProvider);
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
