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

        var texts = new TextsBuilder()
            .AddText("RemindModule/Suggestions/Incoming", "cs", "{0}{1}{2}")
            .AddText("RemindModule/Suggestions/Outgoing", "cs", "{0}{1}{2}")
            .Build();

        return new RemindService(DatabaseBuilder, texts);
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

    [TestMethod]
    public async Task GetRemindSuggestionsAsync()
    {
        var remind = new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(User),
            FromUserId = User.Id.ToString(),
            Id = 12536358627,
            Message = "Message",
            OriginalMessageId = "12345",
            ToUser = Database.Entity.User.FromDiscord(User),
            ToUserId = User.Id.ToString(),
            Language = "cs"
        };
        await Repository.AddAsync(remind);
        await Repository.CommitAsync();

        var suggestions = await Service.GetRemindSuggestionsAsync(User, "cs");
        Assert.AreEqual(1, suggestions.Count);
    }
}
