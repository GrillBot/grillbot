using Discord;
using GrillBot.App.Actions.Commands.Birthday;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Birthday;

[TestClass]
public class HaveBirthdayTests : CommandActionTest<HaveBirthday>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override HaveBirthday CreateInstance()
    {
        return InitAction(new HaveBirthday(DatabaseBuilder));
    }

    private async Task InitDataAsync(bool withBirtday)
    {
        var user = Database.Entity.User.FromDiscord(User);
        user.Birthday = withBirtday ? DateTime.Now : null;

        await Repository.AddAsync(user);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NoUser()
    {
        var result = await Instance.ProcessAsync();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProcessAsync_NoBirthday()
    {
        await InitDataAsync(false);
        var result = await Instance.ProcessAsync();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ProcessAsync_HaveBirthday()
    {
        await InitDataAsync(true);
        var result = await Instance.ProcessAsync();

        Assert.IsTrue(result);
    }
}
