using Discord;
using GrillBot.App.Services.Emotes;
using GrillBot.Tests.Infrastructure.Discord;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Common;

namespace GrillBot.Tests.App.Services.Emotes;

[TestClass]
public class EmotesCommandServiceTests : ServiceTest<EmotesCommandService>
{
    private IGuild Guild { get; set; }

    protected override EmotesCommandService CreateService()
    {
        Guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        var dcClient = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        return new EmotesCommandService(TestServices.EmptyProvider.Value, DatabaseBuilder, dcClient);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [ExcludeFromCodeCoverage]
    public async Task GetInfoAsync_NotEmote()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Service.GetInfoAsync(Emojis.Ok, user);
    }

    [TestMethod]
    public async Task GetInfoAsync_Emote_WithData()
    {
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = "<a:PepeJAMJAM:600070651814084629>",
            FirstOccurence = DateTime.MinValue,
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            GuildId = Guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = Database.Entity.GuildUser.FromDiscord(Guild, user),
            UserId = user.Id.ToString()
        });
        await Repository.CommitAsync();

        var emote = Emote.Parse("<a:PepeJAMJAM:600070651814084629>");
        var result = await Service.GetInfoAsync(emote, user);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetInfoAsync_Emote_WithoutData()
    {
        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        var emote = Emote.Parse("<a:PepeJAMJAM:600070651814084629>");
        var result = await Service.GetInfoAsync(emote, user);

        Assert.IsNull(result);
    }
}
