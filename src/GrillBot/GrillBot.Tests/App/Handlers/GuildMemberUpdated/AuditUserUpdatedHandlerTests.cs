using Discord;
using Discord.Rest;
using GrillBot.App.Handlers.GuildMemberUpdated;
using GrillBot.App.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildMemberUpdated;

[TestClass]
public class AuditUserUpdatedHandlerTests : HandlerTest<AuditUserUpdatedHandler>
{
    protected override AuditUserUpdatedHandler CreateHandler()
    {
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);
        return new AuditUserUpdatedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await Handler.ProcessAsync(user, user);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAuditLogs()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry>()).Build();
        var before = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var after = new GuildUserBuilder(before).SetNickname(Consts.Nickname).SetGuild(guild).Build();

        await Handler.ProcessAsync(before, after);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var memberInfo = ReflectionHelper.CreateWithInternalConstructor<MemberInfo>(Consts.Nickname, null, null);
        var data = ReflectionHelper.CreateWithInternalConstructor<MemberUpdateAuditLogData>(user, memberInfo, memberInfo);
        var entry = new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetActionType(ActionType.MemberUpdated).SetData(data).SetUser(user).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new[] { entry }).Build();
        var before = new GuildUserBuilder(user).Build();
        var after = new GuildUserBuilder(user).SetNickname(Consts.Nickname).SetGuild(guild).Build();

        await Handler.ProcessAsync(before, after);
    }
}
