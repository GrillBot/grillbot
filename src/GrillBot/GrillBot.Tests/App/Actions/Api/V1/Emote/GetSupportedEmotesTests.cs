using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class GetSupportedEmotesTests : ApiActionTest<GetSupportedEmotes>
{
    protected override GetSupportedEmotes CreateAction()
    {
        var emote = EmoteHelper.CreateGuildEmote(Discord.Emote.Parse(Consts.PepeJamEmote));
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

        var emotesCache = new EmotesCacheBuilder()
            .AddEmote(emote, guild)
            .Build();

        return new GetSupportedEmotes(ApiRequestContext, emotesCache, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process();
        Assert.AreEqual(1, result.Count);
    }
}
