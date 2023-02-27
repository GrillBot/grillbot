using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Helpers;
using GrillBot.App.Managers;
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
    private IGuild[] Guilds { get; set; }
    private IGuildUser User { get; set; }
    private IRole Role { get; set; }

    protected override RemoveUnverify CreateInstance()
    {
        Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetSendMessageAction(message).Build();
        Guilds = new[]
        {
            guildBuilder.SetGetUsersAction(new[] { User }).SetGetTextChannelsAction(Enumerable.Empty<ITextChannel>()).Build(),
            new GuildBuilder(Consts.GuildId + 1, Consts.GuildName).Build()
        };

        var client = new ClientBuilder()
            .SetSelfUser(new SelfUserBuilder(User).Build())
            .SetGetGuildsAction(new[] { Guilds[0] })
            .Build();

        var texts = TestServices.Texts.Value;
        var discordClient = TestServices.DiscordSocketClient.Value;
        var unverifyLogger = new UnverifyLogManager(client, DatabaseBuilder);
        var interactions = DiscordHelper.CreateInteractionService(discordClient);
        var loggingManager = new LoggingManager(discordClient, interactions, TestServices.Provider.Value);
        var messageGenerator = new UnverifyMessageManager(texts);
        var unverifyHelper = new UnverifyHelper(DatabaseBuilder);

        return new RemoveUnverify(ApiRequestContext, client, texts, DatabaseBuilder, messageGenerator, unverifyLogger, loggingManager, unverifyHelper);
    }

    private async Task InitDataAsync(bool setUnverify, bool excludeLogItem, bool excludeLogItemData)
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        foreach (var guild in Guilds)
        {
            await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, User));
            await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        }

        if (setUnverify)
        {
            if (!excludeLogItem)
            {
                await Repository.AddAsync(new Database.Entity.UnverifyLog
                {
                    Data = excludeLogItemData
                        ? ""
                        : JsonConvert.SerializeObject(new UnverifyLogSet
                        {
                            End = DateTime.Now,
                            Reason = "Reason",
                            Start = DateTime.Now,
                            ChannelsToKeep = new List<ChannelOverride>(),
                            ChannelsToRemove = new List<ChannelOverride>(),
                            IsSelfUnverify = false,
                            RolesToKeep = new List<ulong>(),
                            RolesToRemove = new List<ulong> { Role.Id },
                            Language = "cs"
                        }),
                    Operation = UnverifyOperation.Unverify,
                    CreatedAt = DateTime.Now,
                    GuildId = Consts.GuildId.ToString(),
                    FromUserId = Consts.UserId.ToString(),
                    ToUserId = Consts.UserId.ToString(),
                    Id = 1
                });
            }

            await Repository.AddCollectionAsync(new[]
            {
                new Database.Entity.Unverify
                {
                    Channels = new List<Database.Entity.GuildChannelOverride>(),
                    Reason = "Reason",
                    Roles = new List<string> { Role.Id.ToString() },
                    EndAt = DateTime.Now,
                    GuildId = Consts.GuildId.ToString(),
                    StartAt = DateTime.Now,
                    UserId = Consts.UserId.ToString(),
                    SetOperationId = excludeLogItem ? 0 : 1
                },
                new Database.Entity.Unverify
                {
                    Channels = new List<Database.Entity.GuildChannelOverride>(),
                    Reason = "Reason",
                    Roles = new List<string> { Role.Id.ToString() },
                    EndAt = DateTime.Now,
                    GuildId = (Consts.GuildId + 1).ToString(),
                    StartAt = DateTime.Now,
                    UserId = Consts.UserId.ToString(),
                    SetOperationId = 0
                }
            });
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() =>
        await Instance.ProcessAsync(Consts.GuildId + 1, Consts.UserId);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DestUserNotFound()
        => await Instance.ProcessAsync(Consts.GuildId, Consts.UserId + 1);

    [TestMethod]
    public async Task ProcessAsync_WithoutUnverify()
    {
        await InitDataAsync(false, true, true);

        var result = await Instance.ProcessAsync(Consts.GuildId, Consts.UserId);
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234");
    }

    [TestMethod]
    public async Task ProcessAsync_FailedReconstruction()
    {
        await InitDataAsync(true, false, true);

        var result = await Instance.ProcessAsync(Consts.GuildId, Consts.UserId);
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "(Missing log data for unverify reconstruction.)");
    }

    [TestMethod]
    public async Task ProcessAsync_Success() => await ProcessSuccessAsync(false);

    [TestMethod]
    public async Task ProcessAsync_Autoremove()
    {
        await InitDataAsync(true, false, false);
        await Instance.ProcessAutoRemoveAsync(Consts.GuildId, Consts.UserId);
    }

    [TestMethod]
    public async Task ProcessAsync_Autoremove_AfterFail()
    {
        await InitDataAsync(true, false, false);
        await Instance.ProcessAutoRemoveAsync(Consts.GuildId + 1, Consts.UserId);
    }

    [TestMethod]
    public async Task ProcessAsync_ForceRemove() => await ProcessSuccessAsync(true);

    private async Task ProcessSuccessAsync(bool force)
    {
        await InitDataAsync(true, false, false);
        var result = await Instance.ProcessAsync(Consts.GuildId, Consts.UserId, force);
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234");
    }
}
