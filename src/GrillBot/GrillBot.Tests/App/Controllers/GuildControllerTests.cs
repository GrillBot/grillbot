using GrillBot.App.Controllers;
using GrillBot.App.Services.Guild;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class GuildControllerTests : ControllerTest<GuildController>
{
    protected override bool CanInitProvider() => false;

    protected override GuildController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new GuildApiService(DatabaseBuilder, discordClient, mapper, CacheBuilder);

        return new GuildController(apiService);
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithFilter()
    {
        var filter = new GetGuildListParams { NameQuery = "Guild" };
        var result = await AdminController.GetGuildListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildListAsync_WithoutFilter()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.CommitAsync();

        var result = await AdminController.GetGuildListAsync(new GetGuildListParams());
        CheckResult<OkObjectResult, PaginatedResponse<Guild>>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.CommitAsync();

        var result = await AdminController.GetGuildDetailAsync(Consts.GuildId);
        CheckResult<OkObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task GetGuildDetailAsync_NotFound()
    {
        var result = await AdminController.GetGuildDetailAsync(Consts.GuildId);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_DiscordGuildNotFound()
    {
        var parameters = new UpdateGuildParams
        {
            AdminChannelId = Consts.ChannelId.ToString(),
            MuteRoleId = Consts.RoleId.ToString(),
        };

        var result = await AdminController.UpdateGuildAsync(Consts.GuildId, parameters);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }
}
