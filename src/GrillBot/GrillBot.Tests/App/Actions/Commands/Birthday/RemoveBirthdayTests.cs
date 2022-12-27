using Discord;
using GrillBot.App.Actions.Commands.Birthday;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Birthday;

 [TestClass]
public class RemoveBirthdayTests : CommandActionTest<RemoveBirthday>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override RemoveBirthday CreateAction()
    {
        return InitAction(new RemoveBirthday(DatabaseBuilder));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_UserNotFound()
    {
        await Action.ProcessAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        await Action.ProcessAsync();
    }
}
