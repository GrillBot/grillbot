using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class RecoverStateTests : ApiActionTest<RecoverState>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private IGuildUser AnotherUser { get; set; }
    private IRole Role { get; set; }

    protected override RecoverState CreateAction()
    {
        Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        var anotherRole = new RoleBuilder(Consts.RoleId + 1, Consts.RoleName).Build();
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetRoles(new[] { anotherRole }).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetRoles(new[] { Role }).Build();
        AnotherUser = new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var texts = new TextsBuilder()
            .AddText("Unverify/Recover/MemberNotFound", "cs", "MemberNotFound{0}")
            .Build();
        var unverifyChecker = new UnverifyChecker(DatabaseBuilder, TestServices.Configuration.Value, TestServices.TestingEnvironment.Value, texts);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(DatabaseBuilder, texts);
        var unverifyLogger = new UnverifyLogger(client, DatabaseBuilder);
        var commandService = DiscordHelper.CreateCommandsService();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, commandService, interactions, ServiceProvider);
        var messageGenerator = new UnverifyMessageGenerator(texts);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, unverifyLogger, DatabaseBuilder, loggingManager, texts, messageGenerator, client);

        return new RecoverState(ApiRequestContext, unverifyService);
    }

    private async Task InitDataAsync(bool includeUnverify = false, bool invalidUser = false)
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(AnotherUser));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, AnotherUser));

        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            Data = JsonConvert.SerializeObject(new UnverifyLogSet
            {
                ChannelsToKeep = new List<ChannelOverride> { new() }, ChannelsToRemove = new List<ChannelOverride> { new() }, RolesToKeep = new List<ulong> { 0 },
                RolesToRemove = new List<ulong> { Role.Id }
            }),
            Operation = UnverifyOperation.Selfunverify,
            CreatedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            ToUserId = (invalidUser ? AnotherUser.Id : User.Id).ToString(),
            Id = 1,
            Unverify = includeUnverify
                ? new Database.Entity.Unverify
                {
                    GuildId = Consts.GuildId.ToString(),
                    UserId = Consts.UserId.ToString(),
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now.AddDays(1),
                }
                : null
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_LogItemNotFound()
        => await Action.ProcessAsync(1);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ValidUnverify()
    {
        await InitDataAsync(true);
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_MemberNotFound()
    {
        await InitDataAsync(false, true);
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        await Action.ProcessAsync(1);
    }
}
