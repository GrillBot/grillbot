using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Channels;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class ChannelControllerTests : ControllerTest<ChannelController>
{
    protected override ChannelController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var mapper = AutoMapperHelper.CreateMapper();
        var dcClient = DiscordHelper.CreateDiscordClient();
        var fileStorage = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService);
        var apiService = new ChannelApiService(DbFactory, mapper, dcClient, messageCache, auditLogService);

        return new ChannelController(apiService);
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_GuildNotFound()
    {
        var result = await AdminController.SendMessageToChannelAsync(122345, 12345, new SendMessageToChannelParams());
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

        var result = await AdminController.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
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
        var result = await AdminController.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
    }

    [TestMethod]
    public async Task ClearChannelCacheAsync()
    {
        var result = await AdminController.ClearChannelCacheAsync(1, 1);
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

        var result = await AdminController.GetChannelDetailAsync(12345, CancellationToken.None);
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

        var result = await AdminController.GetChannelDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_NotFound()
    {
        var result = await AdminController.GetChannelDetailAsync(12345, CancellationToken.None);
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_NotFound()
    {
        var result = await AdminController.UpdateChannelAsync(12345, new UpdateChannelParams());
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

        var result = await AdminController.UpdateChannelAsync(12345, new UpdateChannelParams() { Flags = 42 });
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

        var result = await AdminController.GetChannelUsersAsync(12345, new PaginatedParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<ChannelUserStatItem>>(result);
    }

    [TestMethod]
    public async Task GetChannelboardAsync()
    {
        var result = await AdminController.GetChannelboardAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
