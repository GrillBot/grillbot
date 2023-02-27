using Discord;
using GrillBot.App.Actions.Commands.Images;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class ImageCreatorTests : CommandActionTest<ImageCreator>
{
    protected override IGuild Guild { get; } = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    protected override IGuildUser User { get; } = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override ImageCreator CreateInstance()
    {
        var profilePictureManager = new ProfilePictureManager(CacheBuilder, TestServices.CounterManager.Value);

        var creator = new ImageCreator(profilePictureManager, TestServices.Graphics.Value);
        creator.Init(Context);

        return creator;
    }

    [TestMethod]
    public async Task PeepoloveAsync_SelectedUser() => await ProcessPeepoloveTestAsync(User);

    [TestMethod]
    public async Task PeepoloveAsync_UserFromContext() => await ProcessPeepoloveTestAsync(null);

    [TestMethod]
    public async Task PeepoangryAsync_SelectedUser() => await ProcessPeepoangryTestAsync(User);

    [TestMethod]
    public async Task PeepoangryAsync_UserFromContext() => await ProcessPeepoangryTestAsync(null);

    private async Task ProcessPeepoloveTestAsync(IUser? user)
    {
        using var result = await Instance.PeepoloveAsync(user);
        Assert.IsNotNull(result);
    }

    private async Task ProcessPeepoangryTestAsync(IUser? user)
    {
        using var result = await Instance.PeepoangryAsync(user);
        Assert.IsNotNull(result);
    }
}
