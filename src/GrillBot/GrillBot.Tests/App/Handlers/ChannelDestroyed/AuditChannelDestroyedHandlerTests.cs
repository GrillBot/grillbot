using System.Linq;
using Discord;
using Discord.Rest;
using GrillBot.App.Handlers.ChannelDestroyed;
using GrillBot.App.Services.AuditLog;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelDestroyed;

[TestClass]
public class AuditChannelDestroyedHandlerTests : HandlerTest<AuditChannelDestroyedHandler>
{
    protected override AuditChannelDestroyedHandler CreateHandler()
    {
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new AuditChannelDestroyedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var channel = new DmChannelBuilder().Build();
        await Handler.ProcessAsync(channel);
    }

    [TestMethod]
    public async Task ProcessAsync_AuditLogNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(Enumerable.Empty<IAuditLogEntry>().ToList()).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(channel);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var data = ReflectionHelper.CreateWithInternalConstructor<ChannelDeleteAuditLogData>(Consts.ChannelId, Consts.ChannelName, ChannelType.Text, null, false, null, null);
        var auditLogEntry = new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetUser(user).SetData(data).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry> { auditLogEntry }).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(channel);
    }
}
