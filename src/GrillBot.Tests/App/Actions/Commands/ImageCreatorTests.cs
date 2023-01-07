using System.IO;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class ImageCreatorTests : CommandActionTest<ImageCreator>
{
    protected override IGuild Guild { get; } = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    protected override IGuildUser User { get; } = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override ImageCreator CreateAction()
    {
        var fileStorageFactory = new FileStorageMock(TestServices.Configuration.Value);
        var profilePictureManager = new ProfilePictureManager(CacheBuilder, TestServices.CounterManager.Value);

        var creator = new ImageCreator(fileStorageFactory, profilePictureManager);
        creator.Init(Context);

        return creator;
    }

    private string[] GetFilenames()
        => new[] { $"{User.Id}_{User.Discriminator}_256.png", $"{User.Id}_{User.Discriminator}_64.png" };

    protected override void Cleanup()
    {
        foreach (var file in GetFilenames().Where(File.Exists))
            File.Delete(file);
    }

    [TestMethod]
    public async Task PeepoloveAsync_SelectedUser() => await ProcessPeepoloveTestAsync(User);

    [TestMethod]
    public async Task PeepoloveAsync_UserFromContext() => await ProcessPeepoloveTestAsync(null);

    [TestMethod]
    public async Task PeepoangryAsync_SelectedUser() => await ProcessPeepoangryTestAsync(User);

    [TestMethod]
    public async Task PeepoangryAsync_UserFromContext() => await ProcessPeepoangryTestAsync(null);

    private async Task ProcessPeepoloveTestAsync(IUser user)
    {
        var result = await Action.PeepoloveAsync(user);
        Assert.AreEqual(GetFilenames()[0], Path.GetFileName(result));
    }

    private async Task ProcessPeepoangryTestAsync(IUser user)
    {
        var result = await Action.PeepoangryAsync(user);
        Assert.AreEqual(GetFilenames()[1], Path.GetFileName(result));
    }
}
