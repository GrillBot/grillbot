using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuthControllerTests : ControllerTest<AuthController>
{
    protected override AuthController CreateController()
    {
        // Deps
        var configuration = ConfigurationHelper.CreateConfiguration();
        var dbFactory = new DbContextBuilder();
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, dbFactory, interactions);
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\": \"12345\"}") });
        var service = new OAuth2Service(configuration, dbFactory, loggingService, httpClientFactory);

        return new AuthController(service);
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
}
