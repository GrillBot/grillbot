using GrillBot.App.Controllers;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerTests : ControllerTest<DataController>
{
    protected override DataController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);

        return new DataController(discordClient, DbContext, commandsService, configuration, interactions);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync()
    {
        var result = await AdminController.GetAvailableGuildsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads()
    {
        var result = await AdminController.GetChannelsAsync(12345, false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads()
    {
        var result = await AdminController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithGuild()
    {
        var result = AdminController.GetRoles(12345);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithoutGuild()
    {
        var result = AdminController.GetRoles(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All()
    {
        var result = await AdminController.GetAvailableUsersAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots()
    {
        var result = await AdminController.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Users()
    {
        await DbContext.AddAsync(new User()
        {
            Id = "012345",
            Username = "Username",
            Discriminator = "1234"
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync_AsUser()
    {
        var result = await UserController.GetAvailableGuildsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads_AsUser()
    {
        var result = await UserController.GetChannelsAsync(12345, false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads_AsUser()
    {
        var result = await UserController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithGuild_AsUser()
    {
        var result = UserController.GetRoles(12345);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithoutGuild_AsUser()
    {
        var result = UserController.GetRoles(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetCommandsList()
    {
        var result = UserController.GetCommandsList();
        CheckResult<OkObjectResult, List<string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All_AsUser()
    {
        var result = await UserController.GetAvailableUsersAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots_AsUser()
    {
        var result = await UserController.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Users_AsUser()
    {
        await DbContext.AddAsync(new User()
        {
            Id = "012345",
            Username = "Username",
            Discriminator = "1234"
        });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }
}
