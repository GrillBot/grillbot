using System.Linq;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using Discord;
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
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, DatabaseBuilder, interactions);
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\": \"12345\"}") });
        var service = new OAuth2Service(configuration, DatabaseBuilder, loggingService, httpClientFactory);

        return new AuthController(service, client);
    }

    [TestMethod]
    public void GetRedirectLink()
    {
        var state = new AuthState();
        var link = Controller.GetRedirectLink(state);
        CheckResult<OkObjectResult, OAuth2GetLink>(link);
    }

    [TestMethod]
    public async Task OnOAuth2CallBackAsync()
    {
        var encodedState = new AuthState().Encode();
        var result = await Controller.OnOAuth2CallbackAsync("code", encodedState, CancellationToken.None);
        CheckResult<RedirectResult>(result);
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
