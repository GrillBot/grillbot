using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class RemoveUnverifyTests : ApiActionTest<RemoveUnverify>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }
    private IRole Role { get; set; }

    protected override RemoveUnverify CreateAction()
    {
        Role = new RoleBuilder().SetIdentity(Consts.RoleId, Consts.RoleName).Build();
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        var message = new UserMessageBuilder().SetId(Consts.MessageId).Build();
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetSendMessageAction(message).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).SetGetChannelsAction(Enumerable.Empty<ITextChannel>()).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        var texts = new TextsBuilder()
            .AddText("Unverify/Message/RemoveAccessUnverifyNotFound", "cs", "RemoveAccessUnverifyNotFound")
            .AddText("Unverify/Message/ManuallyRemoveFailed", "cs", "ManuallyRemoveFailed")
            .AddText("Unverify/Message/ManuallyRemoveToChannel", "cs", "ManuallyRemoveToChannel")
            .AddText("Unverify/Message/PrivateManuallyRemovedUnverify", "cs", "PrivateManuallyRemovedUnverify")
            .Build();

        var discordClient = DiscordHelper.CreateClient();
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

        return new RemoveUnverify(ApiRequestContext, client, unverifyService, texts);
    }

    private async Task InitDataAsync(bool setUnverify, bool excludeLogItem)
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
                Roles = new List<string> { Role.Id.ToString() },
                EndAt = DateTime.Now,
                GuildId = Consts.GuildId.ToString(),
                StartAt = DateTime.Now,
                UnverifyLog = !excludeLogItem
                    ? new Database.Entity.UnverifyLog
                    {
                        Data = JsonConvert.SerializeObject(new UnverifyLogSet
                        {
                            End = DateTime.Now,
                            Reason = "Reason",
                            Start = DateTime.Now,
                            ChannelsToKeep = new List<ChannelOverride>(),
                            ChannelsToRemove = new List<ChannelOverride>(),
                            IsSelfUnverify = false,
                            RolesToKeep = new List<ulong>(),
                            RolesToRemove = new List<ulong> { Role.Id }
                        }),
                        Operation = UnverifyOperation.Unverify,
                        CreatedAt = DateTime.Now,
                        GuildId = Consts.GuildId.ToString(),
                        FromUserId = Consts.UserId.ToString(),
                        ToUserId = Consts.UserId.ToString()
                    }
                    : null,
                UserId = Consts.UserId.ToString()
            });
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() =>
        await Action.ProcessAsync(Consts.GuildId + 1, Consts.UserId);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DestUserNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId + 1);

    [TestMethod]
    public async Task ProcessAsync_WithoutUnverify()
    {
        await InitDataAsync(false, true);

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual("RemoveAccessUnverifyNotFound", result);
    }

    [TestMethod]
    [ApiConfiguration(canInitProvider: true)]
    public async Task ProcessAsync_FailedReconstruction()
    {
        await InitDataAsync(true, true);

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual("ManuallyRemoveFailed", result);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync(true, false);

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.AreEqual("ManuallyRemoveToChannel", result);
    }
}
