using GrillBot.App.Services;
using GrillBot.App.Services.User;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Services;

[TestClass]
public class SearchingServiceTests : ServiceTest<SearchingService>
{
    protected override SearchingService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var userService = new UserService(DbFactory, configuration, discordClient);
        var mapper = AutoMapperHelper.CreateMapper();

        return new SearchingService(discordClient, DbFactory, userService, mapper);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_NoMessage()
    {
        await Service.CreateAsync(null, null, null, null);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_EmptyMessage()
    {
        await Service.CreateAsync(null, null, null, "");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_LongMessage()
    {
        var message = new UserMessageBuilder()
            .SetContent(new string('c', 5000))
            .Build();

        await Service.CreateAsync(null, null, null, message.Content);
    }

    [TestMethod]
    public async Task CreateAsync_Success()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateGuildUser();
        var channel = new ChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();

        await Service.CreateAsync(guild, user, channel, "ahoj");
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RemoveSearchAsync_NotValidUser()
    {
        var guild = DataHelper.CreateGuild();
        var channel = new ChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = DataHelper.CreateGuildUser(id: 654321);
        var anotherUser = DataHelper.CreateGuildUser(id: 123456);

        await DbContext.AddAsync(new SearchItem()
        {
            Channel = GuildChannel.FromDiscord(guild, channel, global::Discord.ChannelType.Text),
            ChannelId = channel.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Id = 42,
            MessageContent = "Ahoj",
            User = Database.Entity.User.FromDiscord(user),
            UserId = user.Id.ToString()
        });

        await DbContext.SaveChangesAsync();
        await Service.RemoveSearchAsync(42, anotherUser);
    }

    [TestMethod]
    public async Task RemoveSearchAsync_Admin()
    {
        var guild = DataHelper.CreateGuild();
        var channel = new ChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = DataHelper.CreateGuildUser(id: 654321);
        var anotherUser = DataHelper.CreateGuildUser(id: 123456);

        await DbContext.AddAsync(new SearchItem()
        {
            Channel = GuildChannel.FromDiscord(guild, channel, global::Discord.ChannelType.Text),
            ChannelId = channel.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Id = 42,
            MessageContent = "Ahoj",
            User = Database.Entity.User.FromDiscord(user),
            UserId = user.Id.ToString()
        });

        var userEntity = Database.Entity.User.FromDiscord(anotherUser);
        userEntity.Flags |= (int)UserFlags.BotAdmin;
        await DbContext.AddAsync(userEntity);

        await DbContext.SaveChangesAsync();
        await Service.RemoveSearchAsync(42, anotherUser);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task RemoveSearchAsync_NotFound()
    {
        var anotherUser = DataHelper.CreateGuildUser(id: 123456);
        await Service.RemoveSearchAsync(42, anotherUser);

        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetSearchListAsync()
    {
        var guild = DataHelper.CreateGuild();
        var channel = DataHelper.CreateTextChannel();

        var result = await Service.GetSearchListAsync(guild, channel, "asdf", 1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetItemsCountAsync()
    {
        var guild = DataHelper.CreateGuild();
        var channel = DataHelper.CreateTextChannel();

        var result = await Service.GetItemsCountAsync(guild, channel, "asdf");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GenerateSuggestionsAsync()
    {
        var guild = DataHelper.CreateGuild();
        var channel = new ChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = DataHelper.CreateGuildUser(id: 654321);

        var suggestions = await Service.GenerateSuggestionsAsync(user, guild, channel);
        Assert.AreEqual(0, suggestions.Count);
    }

    [TestMethod]
    public async Task GenerateSuggestionsAsync_Admin()
    {
        var guild = DataHelper.CreateGuild();
        var channel = new ChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = DataHelper.CreateGuildUser(id: 654321);

        var userEntity = Database.Entity.User.FromDiscord(user);
        userEntity.Flags |= (int)UserFlags.BotAdmin;
        await DbContext.AddAsync(userEntity);

        var suggestions = await Service.GenerateSuggestionsAsync(user, guild, channel);
        Assert.AreEqual(0, suggestions.Count);
    }
}
