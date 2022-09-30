using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Invite;

[TestClass]
public class RefreshMetadataTests : ApiActionTest<RefreshMetadata>
{
    protected override RefreshMetadata CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        var admin = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).SetGuildPermissions(GuildPermissions.All).Build();
        var standardUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build())
            .SetGuildPermissions(new GuildPermissions(viewChannel: true, sendMessages: true)).Build();
        var guildWithoutInvites = new GuildBuilder().SetIdentity(Consts.GuildId + 1, Consts.GuildName + "2").SetGetUsersAction(new[] { admin }).SetGetInvitesAction(Enumerable.Empty<IInviteMetadata>())
            .Build();

        var vanityInvite = new InviteMetadataBuilder().SetCode(Consts.VanityInviteCode).SetGuild(guildBuilder.Build()).Build();
        var invite = new InviteMetadataBuilder().SetCode(Consts.InviteCode).SetGuild(guildBuilder.Build()).Build();
        var withoutGuild = new InviteMetadataBuilder().SetCode(Consts.InviteCode + "X").Build();

        var guild = guildBuilder
            .SetGetUsersAction(new[] { admin, standardUser })
            .SetGetInvitesAction(new[] { vanityInvite, invite, withoutGuild })
            .Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { guild, guildWithoutInvites })
            .SetSelfUser(IsPublic ? new SelfUserBuilder(standardUser).Build() : new SelfUserBuilder(admin).Build())
            .Build();

        var inviteManager = new InviteManager(CacheBuilder, TestServices.CounterManager.Value);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);

        return new RefreshMetadata(ApiRequestContext, client, inviteManager, auditLogWriter);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_StandardUser()
    {
        var result = await Action.ProcessAsync(true);

        Assert.AreEqual(0, result.Sum(o => o.Value));
    }

    [TestMethod]
    public async Task ProcessAsync_Admin()
    {
        var result = await Action.ProcessAsync(false);
        Assert.AreEqual(3, result.Sum(o => o.Value));
    }
}
