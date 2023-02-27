using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Infrastructure.Preconditions.Interactions;

[TestClass]
public class RequireEmoteSuggestionChannelAttributeTests : TestBase<RequireEmoteSuggestionChannelAttribute>
{
    protected override RequireEmoteSuggestionChannelAttribute CreateInstance()
    {
        return new RequireEmoteSuggestionChannelAttribute();
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_NoGuild()
    {
        var context = new InteractionContextBuilder().Build();

        var result = await Instance.CheckRequirementsAsync(context, null!, null!);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_GuildNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();

        var result = await Instance.CheckRequirementsAsync(context, null!, TestServices.Provider.Value);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_MissingConfiguration()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.CommitAsync();

        var result = await Instance.CheckRequirementsAsync(context, null!, TestServices.Provider.Value);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_MissingChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();

        var guildData = Database.Entity.Guild.FromDiscord(guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        var result = await Instance.CheckRequirementsAsync(context, null!, TestServices.Provider.Value);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_Success()
    {
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetTextChannelsAction(new[] { channel }).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();

        var guildData = Database.Entity.Guild.FromDiscord(guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        var result = await Instance.CheckRequirementsAsync(context, null!, TestServices.Provider.Value);
        Assert.IsTrue(result.IsSuccess);
    }
}
