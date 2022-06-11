using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.AutoReply;
using GrillBot.App.Services.Channels;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Common;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class ChannelControllerTests : ControllerTest<ChannelController>
{
    protected override bool CanInitProvider() => false;

    protected override ChannelController CreateController(IServiceProvider provider)
    {
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
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
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var mapper = AutoMapperHelper.CreateMapper();
        var storage = new FileStorageMock(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, storage, initManager);
        var autoReplyService = new AutoReplyService(configuration, discordClient, DbFactory, initManager);
        var apiService = new ChannelApiService(DbFactory, mapper, dcClient, messageCache, auditLogService, autoReplyService);

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
        var guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await DbContext.AddAsync(guild);
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
        var guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });

        var channel = new Database.Entity.GuildChannel() { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() };
        channel.Users.Add(new Database.Entity.GuildUserChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        guild.Channels.Add(channel);

        await DbContext.AddAsync(guild);
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetChannelDetailAsync(Consts.ChannelId, CancellationToken.None);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found_WithoutStats()
    {
        var guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await DbContext.AddAsync(guild);
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
        var guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel() { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await DbContext.AddAsync(guild);
        await DbContext.SaveChangesAsync();

        var result = await AdminController.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams() { Flags = 42 });
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelUsersAsync()
    {
        var guild = new Database.Entity.Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser() { User = new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });

        var channel = new Database.Entity.GuildChannel() { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() };
        channel.Users.Add(new Database.Entity.GuildUserChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        guild.Channels.Add(channel);

        await DbContext.AddAsync(guild);
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
