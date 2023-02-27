using Discord;
using GrillBot.App.Actions.Commands.Birthday;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Birthday;

[TestClass]
public class AddBirthdayTests : CommandActionTest<AddBirthday>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override AddBirthday CreateInstance()
    {
        return InitAction(new AddBirthday(DatabaseBuilder));
    }

    [TestMethod]
    public async Task ProcessAsync_WithInit() => await TestAsync(false);

    [TestMethod]
    public async Task ProcessAsync_WithoutInit() => await TestAsync(true);

    private async Task TestAsync(bool withInit)
    {
        if (withInit)
        {
            await Repository.User.GetOrCreateUserAsync(User);
            await Repository.CommitAsync();
        }

        await Instance.ProcessAsync(new DateTime(2022, 02, 04));
    }
}
