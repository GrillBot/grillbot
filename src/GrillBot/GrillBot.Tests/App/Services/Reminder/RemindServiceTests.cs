using GrillBot.App.Services.Reminder;
using GrillBot.Tests.Infrastructure.Discord;
using Discord;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Reminder;

[TestClass]
public class RemindServiceTests : ServiceTest<RemindService>
{
    private static IUser User { get; set; }

    protected override RemindService CreateService()
    {
        User = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        return new RemindService(DatabaseBuilder);
    }

    [TestMethod]
    public async Task GetRemindersCountAsync()
    {
        var result = await Service.GetRemindersCountAsync(User);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetRemindersAsync()
    {
        var result = await Service.GetRemindersAsync(User, 0);
        Assert.AreEqual(0, result.Count);
    }
}
