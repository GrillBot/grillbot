using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Channels;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Database.Models;

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

        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var autoReplyService = new AutoReplyService(TestServices.Configuration.Value, discordClient, DatabaseBuilder, initManager);
        var apiService = new ChannelApiService(DatabaseBuilder, TestServices.AutoMapper.Value, dcClient, messageCache, autoReplyService, ApiRequestContext, auditLogWriter);

        return new ChannelController(apiService);
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_GuildNotFound()
    {
        var result = await Controller.SendMessageToChannelAsync(Consts.GuildId + 1, Consts.ChannelId, new SendMessageToChannelParams() { Content = "Content" });
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_ChannelNotFound()
    {
        var result = await Controller.SendMessageToChannelAsync(Consts.GuildId, Consts.ChannelId + 1, new SendMessageToChannelParams { Content = "Content" });
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task SendMessageToChannelAsync_Success()
    {
        var data = new SendMessageToChannelParams { Content = "Ahoj, toto je test." };
        var result = await Controller.SendMessageToChannelAsync(Consts.GuildId, Consts.ChannelId, data);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithFilter()
    {
        var filter = new GetChannelListParams
        {
            ChannelType = ChannelType.Text,
            GuildId = Consts.GuildId.ToString(),
            NameContains = Consts.ChannelName[..5]
        };

        var result = await Controller.GetChannelsListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
    }

    [TestMethod]
    public async Task GetChannelsListAsync_WithoutFilter()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var filter = new GetChannelListParams();
        var result = await Controller.GetChannelsListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<GuildChannelListItem>>(result);
    }

    [TestMethod]
    public async Task ClearChannelCacheAsync()
    {
        var result = await Controller.ClearChannelCacheAsync(Consts.GuildId, Consts.ChannelId);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });

        var channel = new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() };
        channel.Users.Add(new Database.Entity.GuildUserChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        guild.Channels.Add(channel);

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.GetChannelDetailAsync(Consts.ChannelId);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_Found_WithoutStats()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.GetChannelDetailAsync(Consts.ChannelId);
        CheckResult<OkObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task GetChannelDetailAsync_NotFound()
    {
        var result = await Controller.GetChannelDetailAsync(Consts.ChannelId);
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_NotFound()
    {
        var result = await Controller.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams());
        CheckResult<NotFoundObjectResult, ChannelDetail>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_Found()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams { Flags = 42 });
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task UpdateChannelAsync_NothingChanged()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });
        guild.Channels.Add(new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() });

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.UpdateChannelAsync(Consts.ChannelId, new UpdateChannelParams());
        CheckResult<ObjectResult>(result);
        CheckForStatusCode(result, 500);
    }

    [TestMethod]
    public async Task GetChannelUsersAsync()
    {
        var guild = new Database.Entity.Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName };
        guild.Users.Add(new Database.Entity.GuildUser { User = new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator } });

        var channel = new Database.Entity.GuildChannel { Name = Consts.ChannelName, ChannelId = Consts.ChannelId.ToString() };
        channel.Users.Add(new Database.Entity.GuildUserChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        guild.Channels.Add(channel);

        await Repository.AddAsync(guild);
        await Repository.CommitAsync();

        var result = await Controller.GetChannelUsersAsync(Consts.ChannelId, new PaginatedParams());
        CheckResult<OkObjectResult, PaginatedResponse<ChannelUserStatItem>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetChannelboardAsync()
    {
        var result = await Controller.GetChannelboardAsync();
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
