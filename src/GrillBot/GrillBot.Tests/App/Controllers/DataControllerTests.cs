using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerTests : ControllerTest<DataController>
{
    protected override DataController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var provider = CanInitProvider() ? ServiceProvider : null;
        var emotesCache = new EmotesCacheService(discordClient);

        return new DataController(emotesCache, TestServices.AutoMapper.Value, provider);
    }

    [TestMethod]
    public void GetSupportedEmotes()
    {
        var result = Controller.GetSupportedEmotes();
        CheckResult<OkObjectResult, List<EmoteItem>>(result);
    }
}
