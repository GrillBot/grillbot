using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Help;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UsersControllerTests : ControllerTest<UsersController>
{
    protected override UsersController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var channelsService = new ChannelService(discordClient, DbFactory, configuration, messageCache);
        var provider = DIHelper.CreateEmptyProvider();
        var helpService = new CommandsHelpService(discordClient, commandsService, channelsService, provider, configuration);
        var memoryCache = CacheHelper.CreateMemoryCache();
        var externalHelpService = new ExternalCommandsHelpService(discordClient, configuration, memoryCache, initializationService, provider);

        return new UsersController(DbContext, discordClient, helpService, externalHelpService);
    }

    public override void Cleanup()
    {
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.Emotes.RemoveRange(DbContext.Emotes.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.AuditLogs.RemoveRange(DbContext.AuditLogs.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithFilter()
    {
        var filter = new GetUserListParams()
        {
            Flags = 1,
            GuildId = "1",
            HaveBirthday = true,
            SortDesc = true,
            UsedInviteCode = "ASDF",
            Username = "User"
        };

        var result = await AdminController.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithoutFilter()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.GuildUsers.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, DataHelper.CreateGuildUser()));
        await DbContext.Guilds.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await DbContext.Emotes.AddAsync(new Database.Entity.EmoteStatisticItem()
        {
            EmoteId = Emote.Parse("<:LP_FeelsHighMan:895331837822500866>").ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var filter = new GetUserListParams();
        var result = await AdminController.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_NotFound()
    {
        var result = await AdminController.GetUserDetailAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_Found()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        await DbContext.GuildUsers.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, DataHelper.CreateGuildUser()));
        await DbContext.Guilds.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await DbContext.Emotes.AddAsync(new Database.Entity.EmoteStatisticItem()
        {
            EmoteId = Emote.Parse("<:LP_FeelsHighMan:895331837822500866>").ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetUserDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_NotFound()
    {
        var result = await AdminController.UpdateUserAsync(1, new UpdateUserParams(), CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_Set()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        var parameters = new UpdateUserParams()
        {
            BotAdmin = true,
            PublicAdminBlocked = true,
            WebAdminAllowed = true
        };
        var result = await AdminController.UpdateUserAsync(2, parameters, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_UnSet()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        var parameters = new UpdateUserParams()
        {
            BotAdmin = false,
            PublicAdminBlocked = false,
            WebAdminAllowed = false
        };
        var result = await AdminController.UpdateUserAsync(2, parameters, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task HearthbeatAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await AdminController.HearthbeatAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task HearthbeatOffAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await AdminController.HearthbeatOffAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task GetCurrentUserDetailAsync_NotFound()
    {
        var result = await UserController.GetCurrentUserDetailAsync(CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetCurrentUserDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "3", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "3", UserId = "0" });
        await DbContext.AddAsync(new Database.Entity.EmoteStatisticItem() { EmoteId = "<:PepeLa:751183558126731274>", UserId = "0", GuildId = "3" });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetCurrentUserDetailAsync(CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task HearthbeatAsync_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await UserController.HearthbeatAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task HearthbeatOffAsync_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await UserController.HearthbeatOffAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task GetAvailableCommandsAsync()
    {
        var result = await UserController.GetAvailableCommandsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<CommandGroup>>(result);
    }
}
