using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuthControllerTests : ControllerTest<AuthController>
{
    protected override bool CanInitProvider() => false;

    protected override AuthController CreateController(IServiceProvider provider)
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, DbFactory, interactions);
        var httpClientFactory = HttpClientHelper.CreateFactory(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"access_token\": \"12345\"}") });
        var service = new OAuth2Service(configuration, DbFactory, loggingService, httpClientFactory);

        return new AuthController(service);
    }

    [TestMethod]
    public void GetRedirectLink()
    {
        var state = new AuthState();
        var link = AdminController.GetRedirectLink(state);
        CheckResult<OkObjectResult, OAuth2GetLink>(link);
    }

    [TestMethod]
    public async Task OnOAuth2CallBackAsync()
    {
        var encodedState = new AuthState().Encode();
        var result = await AdminController.OnOAuth2CallbackAsync("code", encodedState, CancellationToken.None);
        CheckResult<RedirectResult>(result);
    }
}
