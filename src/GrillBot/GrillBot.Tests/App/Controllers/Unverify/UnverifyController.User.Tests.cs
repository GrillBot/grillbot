using GrillBot.App.Controllers;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UnverifyControllerUserTests : ControllerTest<UnverifyController>
{
    protected override UnverifyController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var webHostEnv = EnvironmentHelper.CreateEnv("Production");
        var unverifyChecker = new UnverifyChecker(DbFactory, configuration, webHostEnv);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(DbFactory);
        var logger = new UnverifyLogger(discordClient, DbFactory);
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, DbFactory, interactionService);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, logger, DbFactory, loggingService);

        return new UnverifyController(unverifyService, discordClient, DbContext)
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
        DbContext.Unverifies.RemoveRange(DbContext.Unverifies.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.UnverifyLogs.RemoveRange(DbContext.UnverifyLogs.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_NotFound()
    {
        var result = await Controller.GetCurrentUnverifiesAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_Found()
    {
        await DbContext.Guilds.AddAsync(new Database.Entity.Guild() { Id = "1", Name = "Guild" });
        await DbContext.Users.AddAsync(new Database.Entity.User() { Id = "1", Username = "User", Discriminator = "1" });
        await DbContext.GuildUsers.AddAsync(new Database.Entity.GuildUser() { GuildId = "1", UserId = "1" });
        await DbContext.UnverifyLogs.AddAsync(new Database.Entity.UnverifyLog()
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1
        });
        await DbContext.Unverifies.AddAsync(new Database.Entity.Unverify() { GuildId = "1", UserId = "1", StartAt = DateTime.Now, EndAt = DateTime.MaxValue, SetOperationId = 1 });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetCurrentUnverifiesAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithFilter()
    {
        var filter = new UnverifyLogParams()
        {
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            FromUserId = "1",
            GuildId = "1",
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            SortDesc = true,
            ToUserId = "1"
        };

        var result = await Controller.GetUnverifyLogsAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithoutFilter()
    {
        await DbContext.Guilds.AddAsync(new Database.Entity.Guild() { Id = "1", Name = "Guild" });
        await DbContext.Users.AddAsync(new Database.Entity.User() { Id = "1", Username = "User", Discriminator = "1" });
        await DbContext.GuildUsers.AddAsync(new Database.Entity.GuildUser() { GuildId = "1", UserId = "1" });
        await DbContext.UnverifyLogs.AddAsync(new Database.Entity.UnverifyLog()
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{\"RolesToKeep\":[], \"RolesToRemove\":[], \"ChannelsToKeep\":[], \"ChannelsToRemove\":[]}"
        });
        await DbContext.SaveChangesAsync();

        var filter = new UnverifyLogParams();
        var result = await Controller.GetUnverifyLogsAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }
}
