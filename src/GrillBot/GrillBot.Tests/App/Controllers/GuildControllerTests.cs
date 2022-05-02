using GrillBot.App.Controllers;
using GrillBot.App.Services.Guild;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class GuildControllerTests : ControllerTest<GuildController>
{
    protected override GuildController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new GuildApiService(DbFactory, discordClient, mapper);

        return new GuildController(apiService);
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithFilter()
    {
        var filter = new GetGuildListParams() { NameQuery = "Guild" };
        var result = await AdminController.GetGuildListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetGuildListAsync(new GetGuildListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetGuildDetailAsync(Consts.GuildId, CancellationToken.None);
        CheckResult<OkObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_NotFound()
    {
        var result = await AdminController.GetGuildDetailAsync(Consts.GuildId, CancellationToken.None);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_DiscordGuildNotFound()
    {
        var parameters = new UpdateGuildParams()
        {
            AdminChannelId = Consts.ChannelId.ToString(),
            MuteRoleId = Consts.RoleId.ToString(),
        };

        var result = await AdminController.UpdateGuildAsync(Consts.GuildId, parameters);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }
}
