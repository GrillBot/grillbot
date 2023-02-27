using Discord;
using GrillBot.App.Handlers.UserUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.UserUpdated;

[TestClass]
public class SyncUserUpdatedHandlerTests : TestBase<SyncUserUpdatedHandler>
{
    private IUser Before { get; set; } = null!;
    private IUser After { get; set; } = null!;

    protected override void PreInit()
    {
        Before = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        After = new UserBuilder(Consts.UserId, Consts.Username + "Username", Consts.Discriminator).Build();
    }

    protected override SyncUserUpdatedHandler CreateInstance()
    {
        return new SyncUserUpdatedHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(Before));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoChange()
        => await Instance.ProcessAsync(Before, Before);

    [TestMethod]
    public async Task ProcessAsync_NoUserInDb()
        => await Instance.ProcessAsync(Before, After);

    [TestMethod]
    public async Task ProcessAsync_UserIsInDb()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(Before, After);
    }
}
