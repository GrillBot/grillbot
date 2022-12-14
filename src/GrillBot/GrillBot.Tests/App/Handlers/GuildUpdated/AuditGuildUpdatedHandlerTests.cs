using GrillBot.App.Handlers.GuildUpdated;
using GrillBot.App.Services.AuditLog;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildUpdated;

[TestClass]
public class AuditGuildUpdatedHandlerTests : HandlerTest<AuditGuildUpdatedHandler>
{
    protected override AuditGuildUpdatedHandler CreateHandler()
    {
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new AuditGuildUpdatedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        await Handler.ProcessAsync(guild, guild);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAuditLog()
    {
        var before = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var after = new GuildBuilder(Consts.GuildId, Consts.GuildName + "New").Build();

        await Handler.ProcessAsync(before, after);
    }
}
