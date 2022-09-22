using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DataControllerTests : ControllerTest<DataController>
{
    private IGuild Guild { get; set; }

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
        var provider = CanInitProvider() ? ServiceProvider : null;
        var commandsService = DiscordHelper.CreateCommandsService(provider);
        var interactions = DiscordHelper.CreateInteractionService(discordClient, provider);
        var emotesCache = new EmotesCacheService(discordClient);

        return new DataController(client, commandsService, TestServices.Configuration.Value, interactions, emotesCache, TestServices.AutoMapper.Value, DatabaseBuilder, ApiRequestContext, provider);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithGuild_WithThreads()
    {
        var result = await Controller.GetChannelsAsync(Consts.GuildId + 1);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads()
    {
        var result = await Controller.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithGuild()
    {
        var result = await Controller.GetRolesAsync(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetRoles_WithoutGuild()
    {
        var result = await Controller.GetRolesAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_All()
    {
        var result = await Controller.GetAvailableUsersAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Bots()
    {
        var result = await Controller.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public async Task GetAvailableUsersAsync_Users()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();

        var result = await Controller.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetAvailableUsersAsync_WithMutualGuilds()
    {
        var result = await Controller.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetChannelsAsync_WithGuild_WithThreads_AsUser()
    {
        var result = await Controller.GetChannelsAsync(Consts.GuildId + 1);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetChannelsAsync_WithoutGuild_WithoutThreads_AsUser()
    {
        var result = await Controller.GetChannelsAsync(null, true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetRoles_WithGuild_AsUser()
    {
        var result = await Controller.GetRolesAsync(Consts.GuildId);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetRoles_WithoutGuild_AsUser()
    {
        var result = await Controller.GetRolesAsync(null);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true, true)]
    public void GetCommandsList()
    {
        var result = Controller.GetCommandsList();
        CheckResult<OkObjectResult, List<string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetAvailableUsersAsync_All_AsUser()
    {
        var result = await Controller.GetAvailableUsersAsync();
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetAvailableUsersAsync_Bots_AsUser()
    {
        var result = await Controller.GetAvailableUsersAsync(true);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetAvailableUsersAsync_Users_AsUser()
    {
        var user = new UserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();

        var result = await Controller.GetAvailableUsersAsync(false);
        CheckResult<OkObjectResult, Dictionary<string, string>>(result);
    }

    [TestMethod]
    public void GetSupportedEmotes()
    {
        var result = Controller.GetSupportedEmotes();
        CheckResult<OkObjectResult, List<EmoteItem>>(result);
    }
}
