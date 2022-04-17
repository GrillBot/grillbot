using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class InviteControllerTests : ControllerTest<InviteController>
{
    protected override InviteController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = FileStorageHelper.Create(configuration);
        var mapper = AutoMapperHelper.CreateMapper();
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService, mapper);
        var service = new InviteService(discordClient, DbFactory, auditLogService, mapper);

        return new InviteController(service);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithoutFilter()
    {
        await DbContext.Guilds.AddAsync(new Database.Entity.Guild() { Name = "Guild", Id = "12345" });
        await DbContext.Invites.AddAsync(new Database.Entity.Invite() { Code = "Code", CreatorId = "12345", GuildId = "12345" });

        await DbContext.GuildUsers.AddRangeAsync(new[]
        {
            new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" },
            new Database.Entity.GuildUser() { GuildId = "12345", UserId = "123456", UsedInviteCode = "Code" }
        });

        await DbContext.Users.AddRangeAsync(new[]
        {
            new Database.Entity.User() { Username = "Username", Discriminator = "1234", Id = "12345" },
            new Database.Entity.User() { Username = "Username", Discriminator = "1234", Id = "123456" }
        });

        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetInviteListAsync(new GetInviteListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithFilter()
    {
        var filter = new GetInviteListParams()
        {
            Code = "Code",
            CreatedFrom = System.DateTime.MinValue,
            CreatedTo = System.DateTime.MaxValue,
            CreatorId = "12345",
            GuildId = "12345"
        };

        var result = await AdminController.GetInviteListAsync(filter, CancellationToken.None);
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
