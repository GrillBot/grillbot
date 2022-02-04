using GrillBot.App.Controllers;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Params;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class ChannelControllerTests : ControllerTest<ChannelController>
{
    protected override ChannelController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService);

        return new ChannelController(discordClient, DbContext, messageCache);
    }

    public override void Cleanup()
    {
        DbContext.Channels.RemoveRange(DbContext.Channels.AsEnumerable());
        DbContext.UserChannels.RemoveRange(DbContext.UserChannels.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_GuildNotFound()
    {
        var result = await Controller.SendMessageToChannelAsync(12345, 12345, new SendMessageToChannelParams());
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithFilter()
    {
        var filter = new GetChannelListParams()
        {
            ChannelType = Discord.ChannelType.Text,
            GuildId = "12345",
            NameContains = "Channel"
        };

        var result = await Controller.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannel>>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var filter = new GetChannelListParams();
        var result = await Controller.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannel>>(result);
    }

    [TestMethod]
    public async Task ClearChannelCacheAsync()
    {
        var result = await Controller.ClearChannelCacheAsync(1, 1, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Database.Entity.GuildUserChannel() { ChannelId = "12345", GuildId = "12345", UserId = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetChannelDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found_WithoutStats()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetChannelDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_NotFound()
    {
        var result = await Controller.GetChannelDetailAsync(12345, CancellationToken.None);
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_NotFound()
    {
        var result = await Controller.UpdateChannelAsync(12345, new UpdateChannelParams(), CancellationToken.None);
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.UpdateChannelAsync(12345, new UpdateChannelParams(), CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelUsersAsync()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Database.Entity.GuildUserChannel() { ChannelId = "12345", GuildId = "12345", UserId = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetChannelUsersAsync(12345, new PaginatedParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<ChannelUserStatItem>>(result);
    }

    [TestMethod]
    public async Task GetChannelboardAsync()
    {
        var result = await Controller.GetChannelboardAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
