using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class GetSupportedEmotesTests : ApiActionTest<GetSupportedEmotes>
{
    protected override GetSupportedEmotes CreateInstance()
    {
        var emote = EmoteHelper.CreateGuildEmote(Discord.Emote.Parse(Consts.PepeJamEmote));
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { emote }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { guild }).Build();
        var emoteHelper = new GrillBot.App.Helpers.EmoteHelper(client);

        return new GetSupportedEmotes(ApiRequestContext, TestServices.AutoMapper.Value, emoteHelper);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Instance.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
