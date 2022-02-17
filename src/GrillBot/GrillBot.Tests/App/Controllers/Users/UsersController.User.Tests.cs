using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Help;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UsersControllerUserTests : ControllerTest<UsersController>
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
                        new Claim(ClaimTypes.Role, "User")
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
    public async Task GetCurrentUserDetailAsync_NotFound()
    {
        var result = await Controller.GetCurrentUserDetailAsync(CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetCurrentUserDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "0", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "3", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "3", UserId = "0" });
        await DbContext.AddAsync(new Database.Entity.EmoteStatisticItem() { EmoteId = "<:PepeLa:751183558126731274>", UserId = "0" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetCurrentUserDetailAsync(CancellationToken.None);
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

    [TestMethod]
    public async Task GetAvailableCommandsAsync()
    {
        var result = await Controller.GetAvailableCommandsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<CommandGroup>>(result);
    }
}
