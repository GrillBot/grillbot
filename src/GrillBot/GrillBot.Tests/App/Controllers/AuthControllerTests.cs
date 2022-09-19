using System.Linq;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Discord;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuthControllerTests : ControllerTest<AuthController>
{
    protected override AuthController CreateController()
    {
        var user = new UserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var client = new ClientBuilder()
            .SetGetUserAction(user)
            .SetGetGuildsAction(Enumerable.Empty<IGuild>())
            .Build();

        var configuration = TestServices.Configuration.Value;
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, commandsService, interactions, ServiceProvider);
        var service = new OAuth2Service(configuration, DatabaseBuilder, loggingManager);

        return new AuthController(service, client, ServiceProvider);
    }

    [TestMethod]
    public async Task CreateLoginTokenFromIdAsync_UserNotFound()
    {
        var result = await Controller.CreateLoginTokenFromIdAsync(Consts.UserId + 1, true);
        CheckResult<BadRequestObjectResult, OAuth2LoginToken>(result);
    }

    [TestMethod]
    public async Task CreateLoginTokenFromIdAsync_UserFound()
    {
        var result = await Controller.CreateLoginTokenFromIdAsync(Consts.UserId, true);
        CheckResult<OkObjectResult, OAuth2LoginToken>(result);
    }
}
