using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.User;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SearchingControllerTests : ControllerTest<SearchingController>
{
    protected override bool CanInitProvider() => false;

    protected override SearchingController CreateController(IServiceProvider provider)
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var userService = new UserService(DbFactory, configuration, discordClient);
        var mapper = AutoMapperHelper.CreateMapper();
        var searchingService = new SearchingService(discordClient, DbFactory, userService, mapper);

        return new SearchingController(searchingService);
    }

    [TestMethod]
    public async Task GetSearchListAsync_WithFilter()
    {
        var filter = new GetSearchingListParams()
        {
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString()
        };
        filter.Sort.Descending = true;

        var result = await AdminController.GetSearchListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<SearchingListItem>>(result);
    }

    [TestMethod]
    public async Task GetSearchListAsync_WithFilter_AsUser()
    {
        var filter = new GetSearchingListParams()
        {
            ChannelId = Consts.ChannelId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            UserId = Consts.UserId.ToString()
        };
        filter.Sort.Descending = true;

        var result = await UserController.GetSearchListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<SearchingListItem>>(result);
    }

    [TestMethod]
    public async Task GetSearchListAsync_WithoutFilter()
    {
        var search = new SearchItem()
        {
            User = new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator },
            Channel = new GuildChannel()
            {
                ChannelId = Consts.ChannelId.ToString(),
                ChannelType = Discord.ChannelType.Text,
                GuildId = Consts.GuildId.ToString(),
                Name = Consts.ChannelName
            },
            MessageContent = "Msg",
            Guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName }
        };

        await DbContext.AddAsync(search);
        await DbContext.SaveChangesAsync();

        var filter = new GetSearchingListParams();
        var result = await AdminController.GetSearchListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<SearchingListItem>>(result);
    }

    [TestMethod]
    public async Task RemoveSearchesAsync()
    {
        var result = await AdminController.RemoveSearchesAsync(new[] { 1L });
        CheckResult<OkResult>(result);
    }
}
