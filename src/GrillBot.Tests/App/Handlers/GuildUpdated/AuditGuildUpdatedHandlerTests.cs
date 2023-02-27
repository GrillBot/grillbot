using GrillBot.App.Handlers.GuildUpdated;
using GrillBot.App.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildUpdated;

[TestClass]
public class AuditGuildUpdatedHandlerTests : TestBase<AuditGuildUpdatedHandler>
{
    protected override AuditGuildUpdatedHandler CreateInstance()
    {
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);
        return new AuditGuildUpdatedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        await Instance.ProcessAsync(guild, guild);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAuditLog()
    {
        var before = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var after = new GuildBuilder(Consts.GuildId, Consts.GuildName + "New").Build();

        await Instance.ProcessAsync(before, after);
    }
}
