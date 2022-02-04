using GrillBot.App.Controllers;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UnverifyControllerAdminTests : ControllerTest<UnverifyController>
{
    protected override UnverifyController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var webHostEnv = EnvironmentHelper.CreateEnv("Production");
        var unverifyChecker = new UnverifyChecker(dbFactory, configuration, webHostEnv);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(dbFactory);
        var logger = new UnverifyLogger(discordClient, dbFactory);
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, dbFactory, interactionService);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, logger, dbFactory, loggingService);
        DbContext = dbFactory.Create();

        return new UnverifyController(unverifyService, discordClient, DbContext)
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
    public async Task RemoveUnverifyAsync_GuildNotFound()
    {
        var result = await Controller.RemoveUnverifyAsync(1, 1, CancellationToken.None);
        CheckResult<NotFoundObjectResult, MessageResponse>(result);
    }

    [TestMethod]
    public async Task UpdateUnverifyTimeAsync_GuildNotFound()
    {
        var result = await Controller.UpdateUnverifyTimeAsync(1, 1, DateTime.MaxValue, CancellationToken.None);
        CheckResult<NotFoundObjectResult, MessageResponse>(result);
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

    [TestMethod]
    public async Task RecoverUnverifyAsync_ItemNotFound()
    {
        var result = await Controller.RecoverUnverifyAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task RecoverUnverifyAsync_UnverifyNotExists()
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
        var result = await Controller.RecoverUnverifyAsync(1, CancellationToken.None);
        CheckResult<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task RecoverUnverifyAsync_GuildNotFound()
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
        await DbContext.SaveChangesAsync();

        var result = await Controller.RecoverUnverifyAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }
}
