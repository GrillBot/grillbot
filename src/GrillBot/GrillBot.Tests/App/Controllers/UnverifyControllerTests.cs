using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UnverifyControllerTests : ControllerTest<UnverifyController>
{
    protected override bool CanInitProvider() => false;

    protected override UnverifyController CreateController(IServiceProvider provider)
    {
        var guild = new GuildBuilder()
            .SetName(Consts.GuildName).SetId(Consts.GuildId)
            .Build();

        var user = new GuildUserBuilder()
            .SetId(Consts.UserId).SetGuild(guild)
            .SetUsername(Consts.Username).Build();

        var anotherUser = new GuildUserBuilder()
            .SetId(Consts.UserId + 1).SetGuild(guild)
            .SetUsername(Consts.Username + "2").Build();

        var dcClient = new ClientBuilder()
            .SetGetGuildsAction(new List<IGuild>() { guild })
            .SetGetGuildAction(guild)
            .SetGetUserAction(user)
            .SetGetUserAction(anotherUser)
            .Build();

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
        var permissionsCleaner = new PermissionsCleaner(dcClient);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, logger, DbFactory, loggingService, permissionsCleaner);
        var mapper = AutoMapperHelper.CreateMapper();
        var unverifyApiService = new UnverifyApiService(DbFactory, mapper, dcClient);

        return new UnverifyController(unverifyService, dcClient, mapper, unverifyApiService);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_NotFound()
    {
        var result = await AdminController.GetCurrentUnverifiesAsync(CancellationToken.None);
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

        var result = await AdminController.GetCurrentUnverifiesAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task RemoveUnverifyAsync_GuildNotFound()
    {
        var result = await AdminController.RemoveUnverifyAsync(1, 1);
        CheckResult<NotFoundObjectResult, MessageResponse>(result);
    }

    [TestMethod]
    public async Task UpdateUnverifyTimeAsync_GuildNotFound()
    {
        var result = await AdminController.UpdateUnverifyTimeAsync(1, 1, DateTime.MaxValue);
        CheckResult<NotFoundObjectResult, MessageResponse>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithFilter()
    {
        var filter = new UnverifyLogParams()
        {
            Created = new()
            {
                From = DateTime.MinValue,
                To = DateTime.MaxValue
            },
            FromUserId = "1",
            GuildId = "1",
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            ToUserId = "1"
        };
        filter.Sort.Descending = true;

        var result = await AdminController.GetUnverifyLogsAsync(filter, CancellationToken.None);
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
        var result = await AdminController.GetUnverifyLogsAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }

    [TestMethod]
    public async Task RecoverUnverifyAsync_ItemNotFound()
    {
        var result = await AdminController.RecoverUnverifyAsync(1);
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
        var result = await AdminController.RecoverUnverifyAsync(1);
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

        var result = await AdminController.RecoverUnverifyAsync(1);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_NotFound_AsUser()
    {
        var result = await UserController.GetCurrentUnverifiesAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_Found_AsUser()
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

        var result = await UserController.GetCurrentUnverifiesAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithFilter_AsUser()
    {
        var filter = new UnverifyLogParams()
        {
            Created = new()
            {
                From = DateTime.MinValue,
                To = DateTime.MaxValue
            },
            FromUserId = "1",
            GuildId = "1",
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            ToUserId = "1"
        };
        filter.Sort.Descending = true;

        var result = await UserController.GetUnverifyLogsAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithoutFilter_AsUser()
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
        var result = await UserController.GetUnverifyLogsAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }
}
