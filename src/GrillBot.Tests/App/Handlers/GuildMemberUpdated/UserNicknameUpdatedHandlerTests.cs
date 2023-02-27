using Discord;
using GrillBot.App.Handlers.GuildMemberUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildMemberUpdated;

[TestClass]
public class UserNicknameUpdatedHandlerTests : TestBase<UserNicknameUpdatedHandler>
{
    private IGuildUser UserAfter { get; set; } = null!;
    private IGuild Guild { get; set; } = null!;

    protected override void PreInit()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        UserAfter = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetNickname("User1").SetGuild(Guild).Build();
    }

    protected override UserNicknameUpdatedHandler CreateInstance()
    {
        return new UserNicknameUpdatedHandler(DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
        => await Instance.ProcessAsync(null, UserAfter);

    [TestMethod]
    public async Task ProcessAsync_BothCreated()
    {
        var before = new GuildUserBuilder(UserAfter).SetNickname("User2").SetGuild(Guild).Build();
        await Instance.ProcessAsync(before, UserAfter);
    }

    [TestMethod]
    public async Task ProcessAsync_OnlyOneCreated()
    {
        var before = new GuildUserBuilder(UserAfter).SetNickname(null).SetGuild(Guild).Build();
        await Instance.ProcessAsync(before, UserAfter);
    }

    [TestMethod]
    public async Task ProcessAsync_BothWithoutNickname()
    {
        var before = new GuildUserBuilder(UserAfter).SetNickname(null).Build();
        await Instance.ProcessAsync(before, before);
    }
}
