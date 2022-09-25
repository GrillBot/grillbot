using GrillBot.App.Services.Reminder;
using GrillBot.Tests.Infrastructure.Discord;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Reminder;

[TestClass]
public class RemindServiceTests : ServiceTest<RemindService>
{
    private static IUser User { get; set; }

    protected override RemindService CreateService()
    {
        User = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .Build();

        var discordClient = new ClientBuilder()
            .SetGetUserAction(User)
            .SetGetGuildsAction(new[] { guild })
            .Build();

        var texts = new TextsBuilder()
            .AddText("RemindModule/Suggestions/Incoming", "cs", "{0}{1}{2}")
            .AddText("RemindModule/Suggestions/Outgoing", "cs", "{0}{1}{2}")
            .Build();

        return new RemindService(discordClient, DatabaseBuilder, TestServices.Configuration.Value, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CreateRemindAsync_NotInFuture()
    {
        await Service.CreateRemindAsync(null, null, DateTime.MinValue, null, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CreateRemindAsync_MinimalTime()
    {
        var at = DateTime.Now.AddSeconds(10);
        await Service.CreateRemindAsync(null, null, at, null, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CreateRemindAsync_EmptyMessage()
    {
        var at = DateTime.Now.AddHours(12);
        await Service.CreateRemindAsync(null, null, at, null, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CreateRemindAsync_LongMessage()
    {
        var at = DateTime.Now.AddHours(12);
        await Service.CreateRemindAsync(null, null, at, new string('-', 2048), 0);
    }

    [TestMethod]
    public async Task CreateRemindAsync_SameUser()
    {
        var at = DateTime.Now.AddDays(1);

        await Service.CreateRemindAsync(User, User, at, "msg", 970428820521893889);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CreateRemindAsync_AnotherUser()
    {
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator.Replace("1", "5")).Build();
        var at = DateTime.Now.AddDays(1);

        await Service.CreateRemindAsync(User, to, at, "msg", 970428820521893889);
        Assert.IsTrue(true);
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
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_NotFound()
    {
        await Service.CopyAsync(42, User);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_SameUser()
    {
        var at = DateTime.Now.AddDays(1);
        var id = await Service.CreateRemindAsync(User, User, at, "msg", 970428820521893889);
        await Service.CopyAsync(id, User);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_Finished()
    {
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();
        await Repository.User.GetOrCreateUserAsync(to);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.MinValue,
            FromUserId = User.Id.ToString(),
            ToUserId = to.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 5
        });

        await Repository.CommitAsync();
        await Service.CopyAsync(5, to);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_UserNotFound()
    {
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();
        var middle = new UserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username + "2", Consts.Discriminator).Build();
        var third = new UserBuilder().SetIdentity(Consts.UserId + 3, Consts.Username + "XX", Consts.Discriminator).Build();

        await Repository.User.GetOrCreateUserAsync(to);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.User.GetOrCreateUserAsync(middle);
        await Repository.User.GetOrCreateUserAsync(third);
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = third.Id.ToString(),
            ToUserId = middle.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await Repository.CommitAsync();
        await Service.CopyAsync(6, to);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_MultipleSame()
    {
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();

        await Repository.User.GetOrCreateUserAsync(to);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = User.Id.ToString(),
            ToUserId = to.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await Repository.CommitAsync();
        await Service.CopyAsync(6, to);
    }

    [TestMethod]
    public async Task GetProcessableReminderIdsAsync()
    {
        var result = await Service.GetRemindIdsForProcessAsync();
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
            ToUserId = User.Id.ToString()
        };
        await Repository.AddAsync(remind);
        await Repository.CommitAsync();

        var suggestions = await Service.GetRemindSuggestionsAsync(User, "cs");
        Assert.AreEqual(1, suggestions.Count);
    }
}
