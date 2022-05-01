using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Channels;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class ChannelControllerTests : ControllerTest<ChannelController>
{
    protected override ChannelController CreateController()
    {
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .SetRoles(Enumerable.Empty<IRole>())
            .Build();

        var user = new UserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username).SetDiscriminator(Consts.Discriminator)
            .Build();

        var dcClient = new ClientBuilder()
            .SetGetGuildAction(guild)
            .SetGetUserAction(user)
            .SetGetGuildsAction(new List<IGuild>() { guild })
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var mapper = AutoMapperHelper.CreateMapper();
        var fileStorage = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService);
        var apiService = new ChannelApiService(DbFactory, mapper, dcClient, messageCache, auditLogService);

        return new ChannelController(apiService);
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_GuildNotFound()
    {
        var result = await AdminController.SendMessageToChannelAsync(Consts.GuildId, Consts.ChannelId, new SendMessageToChannelParams());
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithFilter()
    {
        var filter = new GetChannelListParams()
        {
            ChannelType = ChannelType.Text,
            GuildId = Consts.GuildId.ToString(),
            NameContains = Consts.ChannelName[..5]
        };

        var result = await AdminController.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var filter = new GetChannelListParams();
        var result = await AdminController.GetChannelsListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
    }

    [TestMethod]
    public async Task ClearChannelCacheAsync()
    {
        var result = await AdminController.ClearChannelCacheAsync(Consts.GuildId, Consts.ChannelId);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.AddAsync(new Database.Entity.GuildUserChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetChannelDetailAsync(Consts.ChannelId, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found_WithoutStats()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetChannelDetailAsync(Consts.ChannelId, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_NotFound()
    {
        var result = await AdminController.GetChannelDetailAsync(Consts.ChannelId, CancellationToken.None);
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_NotFound()
    {
        var result = await AdminController.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams());
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams() { Flags = 42 });
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelUsersAsync()
    {
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.AddAsync(new Database.Entity.GuildUserChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetChannelUsersAsync(Consts.ChannelId, new PaginatedParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<ChannelUserStatItem>>(result);
    }

    [TestMethod]
    public async Task GetChannelboardAsync()
    {
        var result = await AdminController.GetChannelboardAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
