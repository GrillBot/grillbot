using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.App.Services.Emotes;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class GetSupportedEmotesTests : ApiActionTest<GetSupportedEmotes>
{
    protected override GetSupportedEmotes CreateAction()
    {
        var client = DiscordHelper.CreateClient();
        var cache = new EmotesCacheService(client);

        return new GetSupportedEmotes(ApiRequestContext, cache, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process();
        Assert.AreEqual(0, result.Count);
    }
}
