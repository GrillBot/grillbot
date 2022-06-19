﻿using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;
using GrillBot.Common.Models;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UnverifyControllerTests : ControllerTest<UnverifyController>
{
    protected override bool CanInitProvider() => false;

    protected override UnverifyController CreateController()
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
            .SetGetGuildsAction(new List<IGuild> { guild })
            .SetGetGuildAction(guild)
            .SetGetUserAction(user)
            .SetGetUserAction(anotherUser)
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var webHostEnv = EnvironmentHelper.CreateEnv("Production");
        var unverifyChecker = new UnverifyChecker(DatabaseBuilder, configuration, webHostEnv);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(DatabaseBuilder);
        var logger = new UnverifyLogger(discordClient, DatabaseBuilder);
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, DatabaseBuilder, interactionService);
        var counter = new CounterManager();
        var logging = LoggingHelper.CreateLogger<PermissionsCleaner>();
        var permissionsCleaner = new PermissionsCleaner(counter, logging);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, logger, DatabaseBuilder, loggingService, permissionsCleaner);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiRequestContext = new ApiRequestContext();
        var unverifyApiService = new UnverifyApiService(DatabaseBuilder, mapper, dcClient, apiRequestContext);

        return new UnverifyController(unverifyService, dcClient, mapper, unverifyApiService, AdminApiRequestContext);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_NotFound()
    {
        var result = await AdminController.GetCurrentUnverifiesAsync();
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{}"
        });
        await Repository.AddAsync(new Database.Entity.Unverify { GuildId = "1", UserId = "1", StartAt = DateTime.Now, EndAt = DateTime.MaxValue, SetOperationId = 1 });
        await Repository.CommitAsync();

        var result = await AdminController.GetCurrentUnverifiesAsync();
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
        var filter = new UnverifyLogParams
        {
            Created = new RangeParams<DateTime?>
            {
                From = DateTime.MinValue,
                To = DateTime.MaxValue
            },
            FromUserId = "1",
            GuildId = "1",
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            ToUserId = "1",
            Sort =
            {
                Descending = true
            }
        };

        var result = await AdminController.GetUnverifyLogsAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithoutFilter()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{\"RolesToKeep\":[], \"RolesToRemove\":[], \"ChannelsToKeep\":[], \"ChannelsToRemove\":[]}"
        });
        await Repository.CommitAsync();

        var filter = new UnverifyLogParams();
        var result = await AdminController.GetUnverifyLogsAsync(filter);
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
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{}"
        });
        await Repository.AddAsync(new Database.Entity.Unverify { GuildId = "1", UserId = "1", StartAt = DateTime.Now, EndAt = DateTime.MaxValue, SetOperationId = 1 });
        await Repository.CommitAsync();
        var result = await AdminController.RecoverUnverifyAsync(1);
        CheckResult<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task RecoverUnverifyAsync_GuildNotFound()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{}"
        });
        await Repository.CommitAsync();

        var result = await AdminController.RecoverUnverifyAsync(1);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_NotFound_AsUser()
    {
        var result = await UserController.GetCurrentUnverifiesAsync();
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetCurrentUnverifiesAsync_Found_AsUser()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{}"
        });
        await Repository.AddAsync(new Database.Entity.Unverify { GuildId = "1", UserId = "1", StartAt = DateTime.Now, EndAt = DateTime.MaxValue, SetOperationId = 1 });
        await Repository.CommitAsync();

        var result = await UserController.GetCurrentUnverifiesAsync();
        CheckResult<OkObjectResult, List<UnverifyUserProfile>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithFilter_AsUser()
    {
        var filter = new UnverifyLogParams
        {
            Created = new RangeParams<DateTime?>
            {
                From = DateTime.MinValue,
                To = DateTime.MaxValue
            },
            FromUserId = "1",
            GuildId = "1",
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            ToUserId = "1",
            Sort =
            {
                Descending = true
            }
        };

        var result = await UserController.GetUnverifyLogsAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }

    [TestMethod]
    public async Task GetUnverifyLogsAsync_WithoutFilter_AsUser()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = "1", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.User { Id = "1", Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "1", UserId = "1" });
        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            CreatedAt = DateTime.UtcNow,
            Operation = Database.Enums.UnverifyOperation.Selfunverify,
            GuildId = "1",
            FromUserId = "1",
            ToUserId = "1",
            Id = 1,
            Data = "{\"RolesToKeep\":[], \"RolesToRemove\":[], \"ChannelsToKeep\":[], \"ChannelsToRemove\":[]}"
        });
        await Repository.CommitAsync();

        var filter = new UnverifyLogParams();
        var result = await UserController.GetUnverifyLogsAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<UnverifyLogItem>>(result);
    }
}
