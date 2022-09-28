using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class GetCurrentUnverifiesTests : ApiActionTest<GetCurrentUnverifies>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override GetCurrentUnverifies CreateAction()
    {
        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .SetGetUserAction(User)
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var texts = new TextsBuilder().Build();
        var unverifyChecker = new UnverifyChecker(DatabaseBuilder, TestServices.Configuration.Value, TestServices.TestingEnvironment.Value, texts);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(DatabaseBuilder, texts);
        var unverifyLogger = new UnverifyLogger(client, DatabaseBuilder);
        var permissionsCleaner = new PermissionsCleaner(TestServices.CounterManager.Value, LoggingHelper.CreateLogger<PermissionsCleaner>());
        var commandService = DiscordHelper.CreateCommandsService();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, commandService, interactions, ServiceProvider);
        var messageGenerator = new UnverifyMessageGenerator(texts);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, unverifyLogger, DatabaseBuilder, permissionsCleaner, loggingManager, texts,
            messageGenerator, client);

        return new GetCurrentUnverifies(ApiRequestContext, unverifyService, TestServices.AutoMapper.Value);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));

        await Repository.AddAsync(new Database.Entity.UnverifyLog
        {
            Data = JsonConvert.SerializeObject(new UnverifyLogSet()),
            Id = 1,
            Operation = UnverifyOperation.Selfunverify,
            Unverify = new Database.Entity.Unverify
            {
                GuildId = Consts.GuildId.ToString(),
                UserId = Consts.UserId.ToString(),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1),
                SetOperationId = 1
            },
            CreatedAt = DateTime.Now,
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Private()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
