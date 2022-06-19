using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerTests : ControllerTest<DataController>
{
    protected override bool CanInitProvider() => true;

    protected override DataController CreateController(IServiceProvider provider)
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService(provider);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var interactions = DiscordHelper.CreateInteractionService(discordClient, provider);
        var mapper = AutoMapperHelper.CreateMapper();
        var emotesCache = new EmotesCacheService(discordClient);

        return new DataController(discordClient, DbContext, commandsService, configuration, interactions, emotesCache, mapper, DbFactory);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync()
    {
        var result = await AdminController.GetAvailableGuildsAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads()
    {
        var result = await AdminController.GetChannelsAsync(Consts.GuildId, false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads()
    {
        var result = await AdminController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithGuild()
    {
        var result = AdminController.GetRoles(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithoutGuild()
    {
        var result = AdminController.GetRoles(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All()
    {
        var result = await AdminController.GetAvailableUsersAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots()
    {
        var result = await AdminController.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Users()
    {
        var user = new UserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .Build();

        await DbContext.AddAsync(User.FromDiscord(user));
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync_AsUser()
    {
        var result = await UserController.GetAvailableGuildsAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads_AsUser()
    {
        var result = await UserController.GetChannelsAsync(Consts.GuildId, false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads_AsUser()
    {
        var result = await UserController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithGuild_AsUser()
    {
        var result = UserController.GetRoles(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetRoles_WithoutGuild_AsUser()
    {
        var result = UserController.GetRoles(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetCommandsList()
    {
        var result = UserController.GetCommandsList();
        CheckResult<OkObjectResult, List<string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All_AsUser()
    {
        var result = await UserController.GetAvailableUsersAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots_AsUser()
    {
        var result = await UserController.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Users_AsUser()
    {
        var user = new UserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .Build();

        await DbContext.AddAsync(User.FromDiscord(user));
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetSupportedEmotes()
    {
        var result = AdminController.GetSupportedEmotes();
        CheckResult<OkObjectResult, List<EmoteItem>>(result);
    }
}
