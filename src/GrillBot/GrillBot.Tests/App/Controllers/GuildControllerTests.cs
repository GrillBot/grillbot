using GrillBot.App.Controllers;
using GrillBot.App.Services.Guild;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using Discord;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class GuildControllerTests : ControllerTest<GuildController>
{
    protected override bool CanInitProvider() => false;

    protected override GuildController CreateController()
    {
        var guildBuilder = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName);

        var guildChannel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .SetGuild(guildBuilder.Build())
            .Build();

        var role = new RoleBuilder()
            .SetIdentity(Consts.RoleId, Consts.RoleName)
            .Build();

        var guild = guildBuilder
            .SetGetUsersAction(Array.Empty<IGuildUser>())
            .SetGetRoleAction(role)
            .SetGetTextChannelAction(guildChannel)
            .Build();

        var client = new ClientBuilder()
            .SetGetGuildAction(guild)
            .Build();

        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new GuildApiService(DatabaseBuilder, client, mapper, CacheBuilder);

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

        var result = await AdminController.UpdateGuildAsync(Consts.GuildId + 1, parameters);
        CheckResult<NotFoundObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_Success()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.CommitAsync();

        var parameters = new UpdateGuildParams
        {
            AdminChannelId = Consts.ChannelId.ToString(),
            MuteRoleId = Consts.RoleId.ToString(),
            EmoteSuggestionChannelId = Consts.ChannelId.ToString()
        };

        var result = await AdminController.UpdateGuildAsync(Consts.GuildId, parameters);
        CheckResult<OkObjectResult, GuildDetail>(result);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_ValidationFailed()
    {
        await Repository.AddAsync(new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.CommitAsync();

        var parameters = new UpdateGuildParams
        {
            AdminChannelId = (Consts.ChannelId + 1).ToString(),
            MuteRoleId = (Consts.RoleId + 1).ToString(),
            EmoteSuggestionChannelId = (Consts.ChannelId + 1).ToString()
        };

        var result = await AdminController.UpdateGuildAsync(Consts.GuildId, parameters);
        CheckResult<BadRequestObjectResult, GuildDetail>(result);
    }
}
