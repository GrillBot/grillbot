using Discord;
using GrillBot.App.Services.Emotes;
using GrillBot.Data;
using GrillBot.Database.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Emotes;

[TestClass]
public class EmotesCommandServiceTests : ServiceTest<EmotesCommandService>
{
    protected override EmotesCommandService CreateService()
    {
        var serviceProvider = DIHelper.CreateEmptyProvider();
        var dcClient = DiscordHelper.CreateDiscordClient();

        return new EmotesCommandService(serviceProvider, DbFactory, dcClient);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [ExcludeFromCodeCoverage]
    public async Task GetInfoAsync_NotEmote()
    {
        await Service.GetInfoAsync(Emojis.Ok, DataHelper.CreateDiscordUser());
    }

    [TestMethod]
    public async Task GetInfoAsync_Emote_WithData()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateGuildUser();

        await DbContext.AddAsync(new EmoteStatisticItem()
        {
            EmoteId = "<a:PepeJAMJAM:600070651814084629>",
            FirstOccurence = DateTime.MinValue,
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = GuildUser.FromDiscord(guild, user),
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
        var user = DataHelper.CreateGuildUser();
        var emote = Emote.Parse("<a:PepeJAMJAM:600070651814084629>");
        var result = await Service.GetInfoAsync(emote, user);

        Assert.IsNull(result);
    }
}
