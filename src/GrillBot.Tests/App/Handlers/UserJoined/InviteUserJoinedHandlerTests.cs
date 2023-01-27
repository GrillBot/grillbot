using Discord;
using GrillBot.App.Handlers.UserJoined;
using GrillBot.App.Managers;
using GrillBot.Cache.Entity;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.UserJoined;

[TestClass]
public class InviteUserJoinedHandlerTests : HandlerTest<InviteUserJoinedHandler>
{
    private IDiscordClient DiscordClient { get; set; } = null!;
    private GuildUserBuilder AdminBuilder { get; set; } = null!;
    private GuildBuilder GuildBuilder { get; set; } = null!;

    protected override InviteUserJoinedHandler CreateHandler()
    {
        AdminBuilder = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuildPermissions(GuildPermissions.All);
        GuildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);

        DiscordClient = new ClientBuilder()
            .SetSelfUser(new SelfUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build())
            .Build();
        var inviteManager = new InviteManager(CacheBuilder, TestServices.CounterManager.Value);
        var auditLogWriteManager = new AuditLogWriteManager(DatabaseBuilder);

        return new InviteUserJoinedHandler(DiscordClient, inviteManager, auditLogWriteManager, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await CacheRepository.AddAsync(new InviteMetadata
        {
            Code = Consts.InviteCode,
            GuildId = Consts.GuildId.ToString(),
            Uses = 0,
            CreatedAt = DateTime.Now,
            CreatorId = Consts.UserId.ToString(),
            IsVanity = false
        });
        await CacheRepository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Bot()
    {
        var guild = GuildBuilder.SetGetUsersAction(new[] { AdminBuilder.Build() }).Build();
        var user = AdminBuilder.AsBot().SetGuild(guild).Build();

        await Handler.ProcessAsync(user);
    }

    [TestMethod]
    public async Task ProcessAsync_UnknownInvite()
    {
        var userBuilder = AdminBuilder.SetId(Consts.UserId + 1);
        var guild = GuildBuilder.SetGetUsersAction(new[] { userBuilder.Build(), AdminBuilder.SetId(Consts.UserId).AsBot().Build() })
            .SetGetInvitesAction(Array.Empty<IInviteMetadata>()).Build();
        var user = userBuilder.AsBot(false).SetGuild(guild).Build();

        await Handler.ProcessAsync(user);
    }

    [TestMethod]
    public async Task ProcessAsync_DisapearingInvite()
    {
        await InitDataAsync();

        var userBuilder = AdminBuilder.SetId(Consts.UserId + 1);
        var guild = GuildBuilder.SetGetUsersAction(new[] { userBuilder.Build(), AdminBuilder.SetId(Consts.UserId).AsBot().Build() })
            .SetGetInvitesAction(Array.Empty<IInviteMetadata>()).Build();
        var user = userBuilder.AsBot(false).SetGuild(guild).Build();

        await Handler.ProcessAsync(user);
    }

    [TestMethod]
    public async Task ProcessAsync_StandardInvite()
    {
        await InitDataAsync();

        var userBuilder = AdminBuilder.SetId(Consts.UserId + 1);
        var invite = new InviteMetadataBuilder().SetGuild(GuildBuilder.Build()).SetCode(Consts.InviteCode).SetUses(1).Build();
        var guild = GuildBuilder.SetGetUsersAction(new[] { userBuilder.Build(), AdminBuilder.SetId(Consts.UserId).AsBot().Build() })
            .SetGetInvitesAction(new[] { invite }).Build();
        var user = userBuilder.AsBot(false).SetGuild(guild).Build();

        await Handler.ProcessAsync(user);
    }
}
