using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.App.Services.AuditLog;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class ClearMessageCacheTests : ApiActionTest<ClearMessageCache>
{
    protected override ClearMessageCache CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        var guild = guildBuilder.SetGetTextChannelAction(channel).Build();

        var client = new ClientBuilder().SetGetGuildAction(guild).Build();
        var messageCache = new MessageCacheBuilder().SetClearAllMessagesFromChannel(0).Build();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);

        return new ClearMessageCache(ApiRequestContext, client, messageCache, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_GuildNotFound()
        => await Action.ProcessAsync(Consts.GuildId + 1, Consts.ChannelId);

    [TestMethod]
    public async Task ProcessAsync_ChannelNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId + 1);

    [TestMethod]
    public async Task ProcessAsync_Success()
        => await Action.ProcessAsync(Consts.GuildId, Consts.ChannelId);
}
