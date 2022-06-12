using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class InviteControllerTests : ControllerTest<InviteController>
{
    protected override bool CanInitProvider() => false;

    protected override InviteController CreateController(IServiceProvider provider)
    {
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = new FileStorageMock(configuration);
        var mapper = AutoMapperHelper.CreateMapper();
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initManager);
        var service = new InviteService(discordClient, DbFactory, auditLogService, mapper);

        return new InviteController(service);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithoutFilter()
    {
        var guild = new Database.Entity.Guild() { Name = Consts.GuildName, Id = Consts.GuildId.ToString() };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Username = Consts.Username, Id = Consts.UserId.ToString(), Discriminator = Consts.Discriminator } });
        guild.Users.Add(new Database.Entity.GuildUser()
        {
            User = new Database.Entity.User() { Username = Consts.Username, Id = (Consts.UserId + 1).ToString(), Discriminator = Consts.Discriminator },
            UsedInvite = new Database.Entity.Invite() { Code = Consts.InviteCode, CreatorId = Consts.UserId.ToString(), GuildId = Consts.GuildId.ToString() }
        });

        await DbContext.Guilds.AddAsync(guild);
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetInviteListAsync(new GetInviteListParams());
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithFilter()
    {
        var filter = new GetInviteListParams()
        {
            Code = Consts.InviteCode,
            CreatedFrom = System.DateTime.MinValue,
            CreatedTo = System.DateTime.MaxValue,
            CreatorId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString()
        };

        var result = await AdminController.GetInviteListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task RefreshMetadataAsync()
    {
        var result = await AdminController.RefreshMetadataCacheAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public void GetCurrentMetadataCount()
    {
        var result = AdminController.GetCurrentMetadataCount();
        CheckResult<OkObjectResult, int>(result);

        Assert.AreEqual(0, ((OkObjectResult)result.Result).Value);
    }
}
