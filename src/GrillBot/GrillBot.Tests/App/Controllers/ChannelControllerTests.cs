using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Channels;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class ChannelControllerTests : ControllerTest<ChannelController>
{
    protected override ChannelController CreateController()
    {
        var guildBuilder = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName);

        var channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .SetGuild(guildBuilder.Build())
            .Build();

        var guild = guildBuilder
            .SetGetTextChannelAction(channel)
            .Build();

        var user = new UserBuilder()
            .SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .Build();

        var dcClient = new ClientBuilder()
            .SetGetGuildAction(guild)
            .SetGetUserAction(user)
            .SetGetGuildsAction(new List<IGuild> { guild })
            .Build();

        var apiService = new ChannelApiService(DatabaseBuilder, TestServices.AutoMapper.Value, dcClient, ApiRequestContext);
        return new ChannelController(apiService, ServiceProvider);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetChannelboardAsync()
    {
        var result = await Controller.GetChannelboardAsync();
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
