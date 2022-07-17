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
        var userService = new UserService(DatabaseBuilder, configuration);
        var mapper = AutoMapperHelper.CreateMapper();

        return new SearchingService(discordClient, DatabaseBuilder, userService, mapper);
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
        var content = new string('c', 5000);
        await Service.CreateAsync(null, null, null, content);
    }

    [TestMethod]
    public async Task CreateAsync_Success()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var channel = new TextChannelBuilder().SetId(Consts.ChannelId).SetName(Consts.ChannelName).SetGuild(guild).Build();

        await Service.CreateAsync(guild, user, channel, "ahoj");
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task RemoveSearchAsync_NotValidUser()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var anotherUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new SearchItem
        {
            Channel = GuildChannel.FromDiscord(channel, global::Discord.ChannelType.Text),
            ChannelId = channel.Id.ToString(),
            Guild = Database.Entity.Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Id = 42,
            MessageContent = "Ahoj",
            User = Database.Entity.User.FromDiscord(user),
            UserId = user.Id.ToString()
        });
        await Repository.CommitAsync();
        await Service.RemoveSearchAsync(42, anotherUser);
    }

    [TestMethod]
    public async Task RemoveSearchAsync_Admin()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetId(Consts.ChannelId).SetName(Consts.ChannelName).SetGuild(guild).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var anotherUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new SearchItem
        {
            Channel = GuildChannel.FromDiscord(channel, global::Discord.ChannelType.Text),
            ChannelId = channel.Id.ToString(),
            Guild = Database.Entity.Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            Id = 42,
            MessageContent = "Ahoj",
            User = Database.Entity.User.FromDiscord(user),
            UserId = user.Id.ToString()
        });

        var userEntity = Database.Entity.User.FromDiscord(anotherUser);
        userEntity.Flags |= (int)UserFlags.BotAdmin;
        await Repository.AddAsync(userEntity);

        await Repository.CommitAsync();
        await Service.RemoveSearchAsync(42, anotherUser);
    }

    [TestMethod]
    public async Task RemoveSearchAsync_NotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        await Service.RemoveSearchAsync(42, user);
    }

    [TestMethod]
    public async Task GetSearchListAsync()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        var result = await Service.GetSearchListAsync(guild, channel, "asdf", 1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetItemsCountAsync()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        var result = await Service.GetItemsCountAsync(guild, channel, "asdf");
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GenerateSuggestionsAsync()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new ChannelBuilder().SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        var suggestions = await Service.GenerateSuggestionsAsync(user, guild, channel);
        Assert.AreEqual(0, suggestions.Count);
    }

    [TestMethod]
    public async Task GenerateSuggestionsAsync_Admin()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new ChannelBuilder().SetId(Consts.ChannelId).SetName(Consts.ChannelName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        var userEntity = Database.Entity.User.FromDiscord(user);
        userEntity.Flags |= (int)UserFlags.BotAdmin;
        await Repository.AddAsync(userEntity);
        await Repository.CommitAsync();

        var suggestions = await Service.GenerateSuggestionsAsync(user, guild, channel);
        Assert.AreEqual(0, suggestions.Count);
    }
}
