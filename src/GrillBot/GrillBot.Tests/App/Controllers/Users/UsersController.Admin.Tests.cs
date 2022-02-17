using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UsersControllerAdminTests : ControllerTest<UsersController>
{
    protected override UsersController CreateController()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, dbFactory);
        var channelsService = new ChannelService(discordClient, dbFactory, configuration, messageCache);
        var provider = DIHelper.CreateEmptyProvider();
        var helpService = new CommandsHelpService(discordClient, commandsService, channelsService, provider, configuration);
        var memoryCache = CacheHelper.CreateMemoryCache();
        var externalHelpService = new ExternalCommandsHelpService(discordClient, configuration, memoryCache, initializationService, provider);

        return new UsersController(DbContext, discordClient, helpService, externalHelpService)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Role, "Admin")
                    }))
                }
            }
        };
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

        var result = await Controller.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1", Username = "User", Discriminator = "" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "3", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "3", UserId = "1" });
        await DbContext.SaveChangesAsync();

        var filter = new GetUserListParams();
        var result = await Controller.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_NotFound()
    {
        var result = await Controller.GetUserDetailAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "3", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "3", UserId = "2" });
        await DbContext.AddAsync(new Database.Entity.EmoteStatisticItem() { EmoteId = "<:PepeLa:751183558126731274>", UserId = "2" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetUserDetailAsync(2, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_NotFound()
    {
        var result = await Controller.UpdateUserAsync(1, new UpdateUserParams(), CancellationToken.None);
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
        var result = await Controller.UpdateUserAsync(2, parameters, CancellationToken.None);
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
        var result = await Controller.UpdateUserAsync(2, parameters, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task HearthbeatAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await Controller.HearthbeatAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task HearthbeatOffAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await Controller.HearthbeatOffAsync(CancellationToken.None));
    }
}
