using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class GuildControllerTests : ControllerTest<GuildController>
{
    protected override GuildController CreateController()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();
        var discordClient = DiscordHelper.CreateClient();

        return new GuildController(DbContext, discordClient);
    }

    public override void Cleanup()
    {
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithFilter()
    {
        var filter = new GetGuildListParams() { NameQuery = "Guild" };
        var result = await Controller.GetGuildListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Name" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetGuildListAsync(new GetGuildListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Name" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetGuildDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_NotFound()
    {
        var result = await Controller.GetGuildDetailAsync(12345, CancellationToken.None);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_DiscordGuildNotFound()
    {
        var parameters = new UpdateGuildParams()
        {
            AdminChannelId = "12345",
            MuteRoleId = "12345",
        };

        var result = await Controller.UpdateGuildAsync(12345, parameters, CancellationToken.None);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }
}
