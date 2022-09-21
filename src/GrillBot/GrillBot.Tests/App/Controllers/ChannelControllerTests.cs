using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Channels;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
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

        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var autoReplyService = new AutoReplyService(TestServices.Configuration.Value, discordClient, DatabaseBuilder, initManager);
        var apiService = new ChannelApiService(DatabaseBuilder, TestServices.AutoMapper.Value, dcClient, messageCache, autoReplyService, ApiRequestContext, auditLogWriter);

        return new ChannelController(apiService, ServiceProvider);
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
    [ControllerTestConfiguration(true)]
    public async Task GetChannelboardAsync()
    {
        var result = await Controller.GetChannelboardAsync();
        CheckResult<OkObjectResult, List<ChannelboardItem>>(result);
    }
}
