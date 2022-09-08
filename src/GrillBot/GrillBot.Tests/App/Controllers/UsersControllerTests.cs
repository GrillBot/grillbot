using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Birthday;
using GrillBot.App.Services.Channels;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.DirectApi;
using GrillBot.App.Services.User;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Help;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UsersControllerTests : ControllerTest<UsersController>
{
    protected override UsersController CreateController()
    {
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

        var user = new GuildUserBuilder()
            .SetUsername(Consts.Username).SetId(Consts.UserId)
            .SetGuild(guild).Build();

        var dcClient = new ClientBuilder()
            .SetGetGuildAction(guild)
            .SetGetGuildsAction(new List<IGuild> { guild })
            .SetGetUserAction(user)
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = TestServices.Configuration.Value;
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        var mapper = TestServices.AutoMapper.Value;
        var channelsService = new ChannelService(discordClient, DatabaseBuilder, configuration, messageCache);
        var helpService = new CommandsHelpService(discordClient, commandsService, channelsService, ServiceProvider, configuration);
        var directApi = new DirectApiService(discordClient, configuration, initManager, CacheBuilder);
        var externalHelpService = new ExternalCommandsHelpService(directApi, configuration, ServiceProvider);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new UsersApiService(DatabaseBuilder, mapper, dcClient, ApiRequestContext, auditLogWriter);
        var rubbergodKarmaService = new RubbergodKarmaService(directApi, dcClient, mapper);
        var userHearthbeatService = new UserHearthbeatService(DatabaseBuilder);
        var birthdayService = new BirthdayService(dcClient, DatabaseBuilder);

        return new UsersController(helpService, externalHelpService, apiService, rubbergodKarmaService, ApiRequestContext, userHearthbeatService, birthdayService, configuration);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithFilter()
    {
        var filter = new GetUserListParams
        {
            Flags = 1,
            GuildId = Consts.GuildId.ToString(),
            HaveBirthday = true,
            UsedInviteCode = "ASDF",
            Username = Consts.Username,
            Sort = { Descending = true }
        };

        var result = await Controller.GetUsersListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithoutFilter()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var anotherUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var thirdUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddCollectionAsync(new[]
        {
            Database.Entity.User.FromDiscord(user),
            Database.Entity.User.FromDiscord(anotherUser)
        });
        await Repository.AddCollectionAsync(new[]
        {
            Database.Entity.GuildUser.FromDiscord(guild, thirdUser),
            Database.Entity.GuildUser.FromDiscord(guild, anotherUser)
        });
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Emote.Parse(Consts.FeelsHighManEmote).ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await Repository.CommitAsync();

        var filter = new GetUserListParams();
        var result = await Controller.GetUsersListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_NotFound()
    {
        var result = await Controller.GetUserDetailAsync(Consts.UserId);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_Found()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        var guildUserEntity = Database.Entity.GuildUser.FromDiscord(guild, guildUser);
        guildUserEntity.UsedInviteCode = "A";
        await Repository.AddAsync(guildUserEntity);
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Emote.Parse(Consts.FeelsHighManEmote).ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(channel, ChannelType.Text));
        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            ChannelId = channel.Id.ToString(),
            Count = 1,
            FirstMessageAt = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastMessageAt = DateTime.MaxValue,
            UserId = user.Id.ToString()
        });
        await Repository.AddAsync(new Database.Entity.Invite
        {
            Code = "A",
            CreatedAt = DateTime.MinValue,
            CreatorId = user.Id.ToString(),
            GuildId = guild.Id.ToString()
        });
        await Repository.CommitAsync();

        var result = await Controller.GetUserDetailAsync(Consts.UserId);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_NotFound()
    {
        var result = await Controller.UpdateUserAsync(1, new UpdateUserParams());
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_Set()
    {
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = "User", Discriminator = "1" });
        await Repository.CommitAsync();

        var parameters = new UpdateUserParams
        {
            BotAdmin = true,
            PublicAdminBlocked = true,
            WebAdminAllowed = true
        };
        var result = await Controller.UpdateUserAsync(Consts.UserId, parameters);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_UnSet()
    {
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = "User", Discriminator = "1" });
        await Repository.CommitAsync();

        var parameters = new UpdateUserParams
        {
            BotAdmin = false,
            PublicAdminBlocked = false,
            WebAdminAllowed = false
        };
        var result = await Controller.UpdateUserAsync(Consts.UserId, parameters);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task HearthbeatOffAsync()
    {
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = "User", Discriminator = "1" });
        await Repository.CommitAsync();

        CheckResult<OkResult>(await Controller.HearthbeatOffAsync());
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetCurrentUserDetailAsync_NotFound()
    {
        var result = await Controller.GetCurrentUserDetailAsync();
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetCurrentUserDetailAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = "User", Discriminator = "1" });
        await Repository.AddAsync(new Database.Entity.Guild { Id = "3", Name = "Guild" });
        await Repository.AddAsync(new Database.Entity.GuildUser { GuildId = "3", UserId = Consts.UserId.ToString() });
        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem { EmoteId = "<:PepeLa:751183558126731274>", UserId = Consts.UserId.ToString(), GuildId = "3" });
        await Repository.CommitAsync();

        var result = await Controller.GetCurrentUserDetailAsync();
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task HearthbeatOffAsync_AsUser()
    {
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = "User", Discriminator = "1" });
        await Repository.CommitAsync();

        CheckResult<OkResult>(await Controller.HearthbeatOffAsync());
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task GetAvailableCommandsAsync()
    {
        var result = await Controller.GetAvailableCommandsAsync();
        CheckResult<OkObjectResult, List<CommandGroup>>(result);
    }

    [TestMethod]
    public async Task GetTodayBirthdayInfoAsync()
    {
        var result = await Controller.GetTodayBirthdayInfoAsync();
        CheckResult<OkObjectResult, MessageResponse>(result);
    }
}
