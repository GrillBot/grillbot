using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SearchingControllerTests : ControllerTest<SearchingController>
{
    protected override SearchingController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, dbFactory);
        var searchingService = new SearchingService(discordClient, dbFactory, messageCache);
        DbContext = dbFactory.Create();

        return new SearchingController(searchingService);
    }

    public override void Cleanup()
    {
        DbContext.ChangeTracker.Clear();
        DbContext.SearchItems.RemoveRange(DbContext.SearchItems.AsEnumerable());
        DbContext.Channels.RemoveRange(DbContext.Channels.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetSearchListAsync_WithFilter()
    {
        var filter = new GetSearchingListParams()
        {
            ChannelId = "12345",
            GuildId = "12345",
            UserId = "12345",
            SortDesc = true
        };

        var result = await Controller.GetSearchListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<SearchingListItem>>(result);
    }

    [TestMethod]
    public async Task GetSearchListAsync_WithoutFilter()
    {
        await DbContext.Guilds.AddAsync(new Database.Entity.Guild() { Name = "Guild", Id = "1" });
        await DbContext.Users.AddAsync(new Database.Entity.User() { Username = "User", Id = "1", Discriminator = "1" });
        await DbContext.Channels.AddAsync(new Database.Entity.GuildChannel() { ChannelId = "1", ChannelType = Discord.ChannelType.Text, GuildId = "1", Name = "Channel" });
        await DbContext.SearchItems.AddAsync(new Database.Entity.SearchItem() { UserId = "1", GuildId = "1", ChannelId = "1", MessageContent = "Msg" });
        await DbContext.SaveChangesAsync();

        var filter = new GetSearchingListParams();
        var result = await Controller.GetSearchListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<SearchingListItem>>(result);
    }

    [TestMethod]
    public async Task RemoveSearchesAsync()
    {
        var result = await Controller.RemoveSearchesAsync(new[] { 1L }, CancellationToken.None);
        CheckResult<OkResult>(result);
    }
}
