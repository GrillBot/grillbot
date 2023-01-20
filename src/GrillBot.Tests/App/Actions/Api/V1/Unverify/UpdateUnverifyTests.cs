using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class UpdateUnverifyTests : ApiActionTest<UpdateUnverify>
{
    private IGuild Guild { get; set; } = null!;
    private IGuildUser[] Users { get; set; } = null!;

    protected override UpdateUnverify CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        Users = new[]
        {
            new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetSendMessageAction(message).Build(),
            new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetSendMessageAction(null, true).Build()
        };
        Guild = guildBuilder.SetGetUsersAction(Users).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        var texts = TestServices.Texts.Value;
        var unverifyLogger = new UnverifyLogManager(client, DatabaseBuilder);
        var messageGenerator = new UnverifyMessageManager(texts);

        return new UpdateUnverify(ApiRequestContext, client, texts, DatabaseBuilder, unverifyLogger, messageGenerator);
    }

    private async Task InitDataAsync(bool setUnverify, DateTime endAt)
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        foreach (var user in Users)
        {
            await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
            await Repository.AddAsync(GuildUser.FromDiscord(Guild, user));
        }

        if (setUnverify)
        {
            foreach (var user in Users)
            {
                await Repository.AddAsync(new Database.Entity.Unverify
                {
                    Channels = new List<GuildChannelOverride>(),
                    Reason = "Reason",
                    Roles = new List<string>(),
                    EndAt = endAt,
                    GuildId = Consts.GuildId.ToString(),
                    StartAt = DateTime.Now,
                    UserId = user.Id.ToString(),
                    UnverifyLog = new UnverifyLog
                    {
                        Data = JsonConvert.SerializeObject(new GrillBot.Data.Models.Unverify.UnverifyLogSet
                        {
                            Language = "cs"
                        }),
                        Operation = UnverifyOperation.Selfunverify,
                        CreatedAt = DateTime.Now,
                        GuildId = Consts.GuildId.ToString(),
                        FromUserId = user.Id.ToString(),
                        ToUserId = user.Id.ToString()
                    }
                });
            }
        }

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() =>
        await Action.ProcessAsync(Consts.GuildId + 1, Consts.UserId, new UpdateUnverifyParams());

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DestUserNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId + 2, new UpdateUnverifyParams());

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
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "31. 12. 9999 23:59:59");
    }

    [TestMethod]
    public async Task ProcessAsync_Success_WithReason()
    {
        await InitDataAsync(true, DateTime.Now.AddDays(1));

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId, new UpdateUnverifyParams { EndAt = DateTime.MaxValue, Reason = "Reason" });
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "31. 12. 9999 23:59:59", "Reason");
    }

    [TestMethod]
    public async Task ProcessAsync_Success_WithDisabledDms()
    {
        await InitDataAsync(true, DateTime.Now.AddDays(1));

        var result = await Action.ProcessAsync(Consts.GuildId, Consts.UserId + 1, new UpdateUnverifyParams { EndAt = DateTime.MaxValue, Reason = "Reason" });
        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "31. 12. 9999 23:59:59", "Reason");
    }
}
