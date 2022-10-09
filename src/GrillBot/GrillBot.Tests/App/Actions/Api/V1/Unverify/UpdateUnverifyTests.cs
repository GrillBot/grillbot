using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class UpdateUnverifyTests : ApiActionTest<UpdateUnverify>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override UpdateUnverify CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        var message = new UserMessageBuilder().SetId(Consts.MessageId).Build();
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetSendMessageAction(message).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var texts = new TextsBuilder()
            .AddText("Unverify/Update/UnverifyNotFound", "cs", "UnverifyNotFound")
            .AddText("Unverify/Message/PrivateUpdate", "cs", "PrivateUpdate")
            .AddText("Unverify/Message/UpdateToChannel", "cs", "UpdateToChannel")
            .Build();
        var unverifyChecker = new UnverifyChecker(DatabaseBuilder, TestServices.Configuration.Value, TestServices.TestingEnvironment.Value, texts);
        var unverifyProfileGenerator = new UnverifyProfileGenerator(DatabaseBuilder, texts);
        var unverifyLogger = new UnverifyLogger(client, DatabaseBuilder);
        var commandService = DiscordHelper.CreateCommandsService();
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, commandService, interactions, ServiceProvider);
        var messageGenerator = new UnverifyMessageGenerator(texts);
        var unverifyService = new UnverifyService(discordClient, unverifyChecker, unverifyProfileGenerator, unverifyLogger, DatabaseBuilder, loggingManager, texts, messageGenerator, client);

        return new UpdateUnverify(ApiRequestContext, client, unverifyService, texts);
    }

    private async Task InitDataAsync(bool setUnverify, DateTime endAt)
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));

        if (setUnverify)
        {
            await Repository.AddAsync(new Database.Entity.Unverify
            {
                Channels = new List<Database.Entity.GuildChannelOverride>(),
                Reason = "Reason",
                Roles = new List<string>(),
                EndAt = endAt,
                GuildId = Consts.GuildId.ToString(),
                StartAt = DateTime.Now,
                UserId = Consts.UserId.ToString()
            });
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() =>
        await Action.ProcessAsync(Consts.GuildId + 1, Consts.UserId, null);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DestUserNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId + 1, null);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_UnverifyNotFound()
    {
        await InitDataAsync(false, DateTime.Now);
        await Action.ProcessAsync(Consts.GuildId, Consts.UserId, new UpdateUnverifyParams { EndAt = DateTime.MaxValue });
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_InvalidTime()
    {
        await InitDataAsync(true, DateTime.Now);
        await Action.ProcessAsync(Consts.GuildId, Consts.UserId, new UpdateUnverifyParams { EndAt = DateTime.MaxValue });
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(true, DateTime.Now.AddDays(1));

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId, new UpdateUnverifyParams { EndAt = DateTime.MaxValue });

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual("UpdateToChannel", result);
    }
}
