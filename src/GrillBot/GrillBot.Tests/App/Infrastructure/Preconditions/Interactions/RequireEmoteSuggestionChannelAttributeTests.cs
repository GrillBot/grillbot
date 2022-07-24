using System;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Infrastructure.Preconditions.Interactions;

[TestClass]
public class RequireEmoteSuggestionChannelAttributeTests : ServiceTest<RequireEmoteSuggestionChannelAttribute>
{
    protected override RequireEmoteSuggestionChannelAttribute CreateService() => new();

    private IServiceProvider CreateProvider()
    {
        return new ServiceCollection()
            .AddSingleton<GrillBotDatabaseBuilder>(DatabaseBuilder)
            .BuildServiceProvider();
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_NoGuild()
    {
        var context = new InteractionContextBuilder().SetGuild(null).Build();

        var result = await Service.CheckRequirementsAsync(context, null, null);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_GuildNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();
        var provider = CreateProvider();

        var result = await Service.CheckRequirementsAsync(context, null, provider);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_MissingConfiguration()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();
        var provider = CreateProvider();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.CommitAsync();

        var result = await Service.CheckRequirementsAsync(context, null, provider);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_MissingChannel()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();
        var provider = CreateProvider();

        var guildData = Database.Entity.Guild.FromDiscord(guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        var result = await Service.CheckRequirementsAsync(context, null, provider);
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task CheckRequirementsAsync_Success()
    {
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetTextChannelAction(channel).Build();
        var context = new InteractionContextBuilder().SetGuild(guild).Build();
        var provider = CreateProvider();

        var guildData = Database.Entity.Guild.FromDiscord(guild);
        guildData.EmoteSuggestionChannelId = Consts.ChannelId.ToString();
        await Repository.AddAsync(guildData);
        await Repository.CommitAsync();

        var result = await Service.CheckRequirementsAsync(context, null, provider);
        Assert.IsTrue(result.IsSuccess);
    }
}
