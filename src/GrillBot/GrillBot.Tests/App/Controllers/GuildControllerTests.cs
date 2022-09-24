using GrillBot.App.Controllers;
using GrillBot.App.Services.Guild;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Mvc;
using Discord;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class GuildControllerTests : ControllerTest<GuildController>
{
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
        
        var apiService = new GuildApiService(DatabaseBuilder, client);
        return new GuildController(apiService, ServiceProvider);
    }

    [TestMethod]
    public async Task UpdateGuildAsync_DiscordGuildNotFound()
    {
        var parameters = new UpdateGuildParams
        {
            AdminChannelId = Consts.ChannelId.ToString(),
            MuteRoleId = Consts.RoleId.ToString(),
        };

        var result = await Controller.UpdateGuildAsync(Consts.GuildId + 1, parameters);
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

        var result = await Controller.UpdateGuildAsync(Consts.GuildId, parameters);
        //CheckResult<OkObjectResult, GuildDetail>(result);
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

        var result = await Controller.UpdateGuildAsync(Consts.GuildId, parameters);
        //CheckResult<BadRequestObjectResult, GuildDetail>(result);
    }
}
