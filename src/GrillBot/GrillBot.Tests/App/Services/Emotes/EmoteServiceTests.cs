using Discord;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Emotes;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data;
using GrillBot.Database.Entity;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Services.Emotes;

[TestClass]
public class EmoteServiceTests : ServiceTest<EmoteService>
{
    private static Emote Emote => Emote.Parse("<:LP_FeelsHighMan:895331837822500866>");

    protected override EmoteService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);

        return new EmoteService(discordClient, DbFactory, configuration, messageCache);
    }

    private async Task FillDataAsync()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.GuildUsers.AddAsync(GuildUser.FromDiscord(guild, DataHelper.CreateGuildUser()));
        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.Emotes.AddAsync(new EmoteStatisticItem()
        {
            EmoteId = Emote.ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });

        await DbContext.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetEmoteInfoAsync_NoEmote()
    {
        var user = DataHelper.CreateDiscordUser();

        var emote = await Service.GetEmoteInfoAsync(Emote, user);
        Assert.IsNull(emote);
    }

    [TestMethod]
    public async Task GetEmoteInfoAsync_WithEmote()
    {
        var user = DataHelper.CreateDiscordUser();
        await FillDataAsync();

        var emote = await Service.GetEmoteInfoAsync(Emote, user);

        Assert.IsNotNull(emote);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [ExcludeFromCodeCoverage]
    public void EnsureEmote()
    {
        Service.EnsureEmote(Emojis.Ok, out var _);
    }

    [TestMethod]
    public async Task GetEmoteListAsync_WithUser()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();

        var result = await Service.GetEmoteListAsync(guild, user, "count/asc", 50);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetEmoteListAsync_WithoutUser()
    {
        var guild = DataHelper.CreateGuild();

        var result = await Service.GetEmoteListAsync(guild, null, "count/desc", null);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetEmoteListAsync_LastUseAsc()
    {
        var guild = DataHelper.CreateGuild();

        var result = await Service.GetEmoteListAsync(guild, null, "lastuse/asc", null);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task GetEmoteListAsync_LastUseDesc()
    {
        var guild = DataHelper.CreateGuild();

        var result = await Service.GetEmoteListAsync(guild, null, "lastuse/desc", null);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    [ExcludeFromCodeCoverage]
    public async Task GetEmoteListAsync_Unsupported()
    {
        var guild = DataHelper.CreateGuild();

        await Service.GetEmoteListAsync(guild, null, null, null);
    }

    [TestMethod]
    public async Task GetEmotesCountAsync_WithUser()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();

        await Service.GetEmotesCountAsync(guild, user);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetEmotesCountAsync_WithoutUser()
    {
        var guild = DataHelper.CreateGuild();

        await Service.GetEmotesCountAsync(guild, null);
        Assert.IsTrue(true);
    }
}
