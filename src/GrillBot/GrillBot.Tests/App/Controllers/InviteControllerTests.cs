using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Data.Models.API.Invites;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class InviteControllerTests : ControllerTest<InviteController>
{
    protected override InviteController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var inviteManager = new InviteManager(CacheBuilder, TestServices.CounterManager.Value);
        var service = new InviteService(discordClient, DatabaseBuilder, TestServices.AutoMapper.Value, auditLogWriter, inviteManager);

        return new InviteController(service, ApiRequestContext);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithoutFilter()
    {
        var guild = new Database.Entity.Guild { Name = Consts.GuildName, Id = Consts.GuildId.ToString() };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Username = Consts.Username, Id = Consts.UserId.ToString(), Discriminator = Consts.Discriminator } });
        guild.Users.Add(new Database.Entity.GuildUser
        {
            User = new Database.Entity.User { Username = Consts.Username, Id = (Consts.UserId + 1).ToString(), Discriminator = Consts.Discriminator },
            UsedInvite = new Database.Entity.Invite { Code = Consts.InviteCode, CreatorId = Consts.UserId.ToString(), GuildId = Consts.GuildId.ToString() }
        });

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.GetInviteListAsync(new GetInviteListParams());
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task GetInviteListAsync_WithFilter()
    {
        var filter = new GetInviteListParams
        {
            Code = Consts.InviteCode,
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            CreatorId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString()
        };

        var result = await Controller.GetInviteListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<GuildInvite>>(result);
    }

    [TestMethod]
    public async Task RefreshMetadataAsync()
    {
        var result = await Controller.RefreshMetadataCacheAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetCurrentMetadataCount()
    {
        var result = await Controller.GetCurrentMetadataCountAsync();
        CheckResult<OkObjectResult, int>(result);

        Assert.AreEqual(0, ((OkObjectResult)result.Result)?.Value);
    }
}
