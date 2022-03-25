using GrillBot.App.Controllers;
using GrillBot.Database.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerAdminTests : ControllerTest<DataController>
{
    protected override DataController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);

        return new DataController(discordClient, DbContext, commandsService, configuration, interactions)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Role, "Admin")
                    }))
                }
            }
        };
    }

    public override void Cleanup()
    {
        DbContext.Channels.RemoveRange(DbContext.Channels.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync()
    {
        var result = await Controller.GetAvailableGuildsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads()
    {
        var result = await Controller.GetChannelsAsync(12345, false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads()
    {
        var result = await Controller.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithGuild()
    {
        var result = Controller.GetRoles(12345);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithoutGuild()
    {
        var result = Controller.GetRoles(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All()
    {
        var result = await Controller.GetAvailableUsersAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots()
    {
        var result = await Controller.GetAvailableUsersAsync(true);
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

        var result = await Controller.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }
}
