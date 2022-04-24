using Discord;
using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Channels;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.User;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Help;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class UsersControllerTests : ControllerTest<UsersController>
{
    protected override UsersController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var mapper = AutoMapperHelper.CreateMapper();
        var channelsService = new ChannelService(discordClient, DbFactory, configuration, messageCache);
        var provider = DIHelper.CreateEmptyProvider();
        var helpService = new CommandsHelpService(discordClient, commandsService, channelsService, provider, configuration);
        var memoryCache = CacheHelper.CreateMemoryCache();
        var externalHelpService = new ExternalCommandsHelpService(discordClient, configuration, memoryCache, initializationService, provider);
        var storageFactory = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, storageFactory, initializationService);
        var dcClient = DiscordHelper.CreateDiscordClient();
        var apiService = new UsersApiService(DbFactory, mapper, dcClient, auditLogService);

        return new UsersController(helpService, externalHelpService, apiService);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithFilter()
    {
        var filter = new GetUserListParams()
        {
            Flags = 1,
            GuildId = "1",
            HaveBirthday = true,
            UsedInviteCode = "ASDF",
            Username = "User"
        };
        filter.Sort.Descending = true;

        var result = await AdminController.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUsersListAsync_WithoutFilter()
    {
        var user = DataHelper.CreateDiscordUser();
        var anotherUser = DataHelper.CreateGuildUser(id: user.Id + 1);

        var guild = DataHelper.CreateGuild();

        await DbContext.Users.AddRangeAsync(
            Database.Entity.User.FromDiscord(user),
            Database.Entity.User.FromDiscord(anotherUser)
        );
        await DbContext.GuildUsers.AddRangeAsync(new[]
        {
            Database.Entity.GuildUser.FromDiscord(guild, DataHelper.CreateGuildUser()),
            Database.Entity.GuildUser.FromDiscord(guild, anotherUser)
        });
        await DbContext.Guilds.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await DbContext.Emotes.AddAsync(new Database.Entity.EmoteStatisticItem()
        {
            EmoteId = Emote.Parse("<:LP_FeelsHighMan:895331837822500866>").ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var filter = new GetUserListParams();
        var result = await AdminController.GetUsersListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<UserListItem>>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_NotFound()
    {
        var result = await AdminController.GetUserDetailAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetUserDetailAsync_Found()
    {
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();
        var channel = DataHelper.CreateChannel();

        await DbContext.Users.AddAsync(Database.Entity.User.FromDiscord(user));
        var guildUserEntity = Database.Entity.GuildUser.FromDiscord(guild, DataHelper.CreateGuildUser());
        guildUserEntity.UsedInviteCode = "A";
        await DbContext.GuildUsers.AddAsync(guildUserEntity);
        await DbContext.Guilds.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await DbContext.Emotes.AddAsync(new Database.Entity.EmoteStatisticItem()
        {
            EmoteId = Emote.Parse("<:LP_FeelsHighMan:895331837822500866>").ToString(),
            FirstOccurence = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 50,
            UserId = user.Id.ToString()
        });
        await DbContext.Channels.AddAsync(Database.Entity.GuildChannel.FromDiscord(guild, channel, ChannelType.Text));
        await DbContext.UserChannels.AddAsync(new Database.Entity.GuildUserChannel()
        {
            ChannelId = channel.Id.ToString(),
            Count = 1,
            FirstMessageAt = DateTime.MinValue,
            GuildId = guild.Id.ToString(),
            LastMessageAt = DateTime.MaxValue,
            UserId = user.Id.ToString()
        });
        await DbContext.Invites.AddAsync(new Database.Entity.Invite()
        {
            Code = "A",
            CreatedAt = DateTime.MinValue,
            CreatorId = user.Id.ToString(),
            GuildId = guild.Id.ToString()
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetUserDetailAsync(12345, CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_NotFound()
    {
        var result = await AdminController.UpdateUserAsync(1, new UpdateUserParams());
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_Set()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        var parameters = new UpdateUserParams()
        {
            BotAdmin = true,
            PublicAdminBlocked = true,
            WebAdminAllowed = true
        };
        var result = await AdminController.UpdateUserAsync(2, parameters);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task UpdateUserAsync_UnSet()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "2", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        var parameters = new UpdateUserParams()
        {
            BotAdmin = false,
            PublicAdminBlocked = false,
            WebAdminAllowed = false
        };
        var result = await AdminController.UpdateUserAsync(2, parameters);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task HearthbeatAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await AdminController.HearthbeatAsync());
    }

    [TestMethod]
    public async Task HearthbeatOffAsync()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await AdminController.HearthbeatOffAsync());
    }

    [TestMethod]
    public async Task GetCurrentUserDetailAsync_NotFound()
    {
        var result = await UserController.GetCurrentUserDetailAsync(CancellationToken.None);
        CheckResult<NotFoundObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task GetCurrentUserDetailAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "3", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "3", UserId = "1234567890" });
        await DbContext.AddAsync(new Database.Entity.EmoteStatisticItem() { EmoteId = "<:PepeLa:751183558126731274>", UserId = "1234567890", GuildId = "3" });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetCurrentUserDetailAsync(CancellationToken.None);
        CheckResult<OkObjectResult, UserDetail>(result);
    }

    [TestMethod]
    public async Task HearthbeatAsync_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await UserController.HearthbeatAsync());
    }

    [TestMethod]
    public async Task HearthbeatOffAsync_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.SaveChangesAsync();

        CheckResult<OkResult>(await UserController.HearthbeatOffAsync());
    }

    [TestMethod]
    public async Task GetAvailableCommandsAsync()
    {
        var result = await UserController.GetAvailableCommandsAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<CommandGroup>>(result);
    }

    [TestMethod]
    public async Task GetPointsBoardAsync_WithData()
    {
        await DbContext.AddAsync(new Database.Entity.User() { Id = "1234567890", Username = "User", Discriminator = "1" });
        await DbContext.AddAsync(new Database.Entity.Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new Database.Entity.GuildUser() { GuildId = "12345", UserId = "1234567890", Points = 50 });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetPointsLeaderboardAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }

    [TestMethod]
    public async Task GetPointsBoardAsync_WithoutData()
    {
        var result = await UserController.GetPointsLeaderboardAsync(CancellationToken.None);
        CheckResult<OkObjectResult, List<UserPointsItem>>(result);
    }
}
