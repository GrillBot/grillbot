using Discord;
using GrillBot.App.Services.Emotes;
using GrillBot.Data;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;
using System.Diagnostics.CodeAnalysis;

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

        var serviceProvider = DIHelper.CreateEmptyProvider();
        return new EmotesCommandService(serviceProvider, DbFactory, dcClient);
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

        await DbContext.AddAsync(new Database.Entity.EmoteStatisticItem()
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
        await DbContext.SaveChangesAsync();

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
