using Discord;
using GrillBot.App.Handlers.GuildMemberUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildMemberUpdated;

[TestClass]
public class UserUpdatedSyncHandlerTests : TestBase<UserUpdatedSyncHandler>
{
    private IGuildUser User { get; set; } = null!;

    protected override void PreInit()
    {
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(new GuildBuilder(Consts.GuildId, Consts.GuildName).Build()).Build();
    }

    protected override UserUpdatedSyncHandler CreateInstance()
    {
        return new UserUpdatedSyncHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(User.Guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(User.Guild, User));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        await Instance.ProcessAsync(User, User);
    }

    [TestMethod]
    public async Task ProcessAsync_UserNotFound()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username + "2", Consts.Discriminator).Build();
        await Instance.ProcessAsync(User, user);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username + "2", Consts.Discriminator).SetGuild(User.Guild).Build();

        await InitDataAsync();
        await Instance.ProcessAsync(User, user);
    }
}
