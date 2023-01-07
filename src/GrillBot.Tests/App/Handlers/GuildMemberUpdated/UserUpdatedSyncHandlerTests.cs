using Discord;
using GrillBot.App.Handlers.GuildMemberUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildMemberUpdated;

[TestClass]
public class UserUpdatedSyncHandlerTests : HandlerTest<UserUpdatedSyncHandler>
{
    private IGuildUser User { get; set; }

    protected override UserUpdatedSyncHandler CreateHandler()
    {
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(new GuildBuilder(Consts.GuildId, Consts.GuildName).Build()).Build();
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
        await Handler.ProcessAsync(User, User);
    }

    [TestMethod]
    public async Task ProcessAsync_UserNotFound()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username + "2", Consts.Discriminator).Build();
        await Handler.ProcessAsync(User, user);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username + "2", Consts.Discriminator).SetGuild(User.Guild).Build();

        await InitDataAsync();
        await Handler.ProcessAsync(User, user);
    }
}
