﻿using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerTests : ControllerTest<DataController>
{
    private IGuild Guild { get; set; }
    
    protected override bool CanInitProvider() => true;

    protected override DataController CreateController()
    {
        var guildBuilder = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName);

        var user = new GuildUserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guildBuilder.Build())
            .Build();

        var channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .SetGuild(guildBuilder.Build())
            .Build();

        var role = new RoleBuilder().SetIdentity(Consts.RoleId, Consts.RoleName).SetPosition(1).Build();

        Guild = guildBuilder
            .SetGetUsersAction(new[] { user })
            .SetGetChannelsAction(new[] { channel })
            .SetRoles(new[] { role })
            .Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService(ServiceProvider);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var interactions = DiscordHelper.CreateInteractionService(discordClient, ServiceProvider);
        var mapper = AutoMapperHelper.CreateMapper();
        var emotesCache = new EmotesCacheService(discordClient);

        return new DataController(client, commandsService, configuration, interactions, emotesCache, mapper,
            DatabaseBuilder, ApiRequestContext);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.CommitAsync();
                
        var result = await AdminController.GetAvailableGuildsAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableGuildsAsync_Public()
    {
        SelectApiRequestContext(true);
        ReflectionHelper.SetPrivateReadonlyPropertyValue(UserController, nameof(ApiRequestContext), ApiRequestContext);

        var result = await UserController.GetAvailableGuildsAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads()
    {
        var result = await AdminController.GetChannelsAsync(Consts.GuildId + 1);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads()
    {
        var result = await AdminController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithGuild()
    {
        var result = await AdminController.GetRolesAsync(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithoutGuild()
    {
        var result = await AdminController.GetRolesAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All()
    {
        var result = await AdminController.GetAvailableUsersAsync();
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
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();

        var result = await AdminController.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_WithMutualGuilds()
    {
        SelectApiRequestContext(true);
        ReflectionHelper.SetPrivateReadonlyPropertyValue(UserController, nameof(ApiRequestContext), ApiRequestContext);

        var result = await UserController.GetAvailableUsersAsync(false);
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
        SelectApiRequestContext(true);
        ReflectionHelper.SetPrivateReadonlyPropertyValue(UserController, nameof(ApiRequestContext), ApiRequestContext);

        var result = await UserController.GetChannelsAsync(Consts.GuildId + 1);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads_AsUser()
    {
        SelectApiRequestContext(true);
        ReflectionHelper.SetPrivateReadonlyPropertyValue(UserController, nameof(ApiRequestContext), ApiRequestContext);

        var result = await UserController.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithGuild_AsUser()
    {
        var result = await UserController.GetRolesAsync(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithoutGuild_AsUser()
    {
        var result = await UserController.GetRolesAsync(null);
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
        var result = await UserController.GetAvailableUsersAsync();
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

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();

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