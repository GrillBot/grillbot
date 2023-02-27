using Discord;
using GrillBot.App.Handlers.UserUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.UserUpdated;

[TestClass]
public class SyncUserInServicesHandlerTests : TestBase<SyncUserInServicesHandler>
{
    private IUser Before { get; set; } = null!;

    protected override void PreInit()
    {
        Before = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
    }

    protected override SyncUserInServicesHandler CreateInstance()
    {
        return new SyncUserInServicesHandler(TestServices.RubbergodServiceClient.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutChange()
        => await Instance.ProcessAsync(Before, Before);

    [TestMethod]
    public async Task ProcessAsync_Different()
    {
        var after = new UserBuilder(Consts.UserId, Consts.Username + "New", Consts.Discriminator + 1).SetAvatar(Guid.NewGuid().ToString()).Build();
        await Instance.ProcessAsync(Before, after);
    }
}
