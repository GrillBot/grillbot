using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Reminder;
using GrillBot.Database.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Services.Reminder;

[TestClass]
public class RemindServiceTests : ServiceTest<RemindService>
{
    protected override RemindService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var fileStorage = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService);

        return new RemindService(discordClient, DbFactory, configuration, auditLogService);
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
    public async Task CreateRemindAsync_SameUser()
    {
        var user = DataHelper.CreateDiscordUser();
        var at = DateTime.Now.AddDays(1);
        var msg = DataHelper.CreateMessage();

        await Service.CreateRemindAsync(user, user, at, "msg", msg.Id);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CreateRemindAsync_AnotherUser()
    {
        var from = DataHelper.CreateDiscordUser();
        var to = DataHelper.CreateDiscordUser("Username", 123456, "1235");
        var at = DateTime.Now.AddDays(1);
        var msg = DataHelper.CreateMessage();

        await Service.CreateRemindAsync(from, to, at, "msg", msg.Id);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetRemindersCountAsync()
    {
        var user = DataHelper.CreateDiscordUser();
        var result = await Service.GetRemindersCountAsync(user);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetRemindersAsync()
    {
        var user = DataHelper.CreateDiscordUser();
        var result = await Service.GetRemindersAsync(user, 0);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_NotFound()
    {
        var user = DataHelper.CreateDiscordUser();

        await Service.CopyAsync(42, user);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_SameUser()
    {
        var user = DataHelper.CreateDiscordUser();
        var msg = DataHelper.CreateMessage();

        var at = DateTime.Now.AddDays(1);
        var id = await Service.CreateRemindAsync(user, user, at, "msg", msg.Id);
        await Service.CopyAsync(id, user);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_Finished()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        await DbContext.InitUserAsync(toUser, CancellationToken.None);
        await DbContext.InitUserAsync(DataHelper.CreateDiscordUser(), CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.MinValue,
            FromUserId = "12345",
            ToUserId = "12345",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 5
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(5, toUser);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_UserNotFound()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();

        await DbContext.InitUserAsync(toUser);
        await DbContext.InitUserAsync(fromUser);
        await DbContext.InitUserAsync(DataHelper.CreateDiscordUser("Username", 1234567, "1236"));
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = "12345",
            ToUserId = "1234567",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(6, toUser);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_MultipleSame()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();

        await DbContext.InitUserAsync(toUser);
        await DbContext.InitUserAsync(fromUser);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = "12345",
            ToUserId = "123456",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(6, toUser);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_NotFound()
    {
        await Service.CancelRemindAsync(1, null, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_Finished()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();
        var canceller = DataHelper.CreateDiscordUser("C", 1234567, "1596");

        await DbContext.InitUserAsync(toUser, CancellationToken.None);
        await DbContext.InitUserAsync(fromUser, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.MinValue,
            FromUserId = "12345",
            ToUserId = "12345",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, canceller, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_Finished2()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();
        var canceller = DataHelper.CreateDiscordUser("C", 1234567, "1596");

        await DbContext.InitUserAsync(toUser, CancellationToken.None);
        await DbContext.InitUserAsync(fromUser, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = "12345",
            ToUserId = "12345",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1,
            RemindMessageId = "12345"
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, canceller, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_NoPerms()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();
        var canceller = DataHelper.CreateDiscordUser("C", 1234567, "1596");

        await DbContext.InitUserAsync(toUser, CancellationToken.None);
        await DbContext.InitUserAsync(fromUser, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = "12345",
            ToUserId = "12345",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, canceller, false);
    }

    [TestMethod]
    public async Task CancelRemindAsync_Success()
    {
        var toUser = DataHelper.CreateDiscordUser("Username", 123456, "1236");
        var fromUser = DataHelper.CreateDiscordUser();

        await DbContext.InitUserAsync(toUser, CancellationToken.None);
        await DbContext.InitUserAsync(fromUser, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = "12345",
            ToUserId = "12345",
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();
        await Service.CancelRemindAsync(1, fromUser, false);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetProcessableReminderIdsAsync()
    {
        var result = await Service.GetProcessableReminderIdsAsync();
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetRemindSuggestionsAsync()
    {
        var user = DataHelper.CreateDiscordUser(id: 48286286392);

        var remind = new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(user),
            FromUserId = user.Id.ToString(),
            Id = 12536358627,
            Message = "Message",
            OriginalMessageId = "12345",
            ToUser = Database.Entity.User.FromDiscord(user),
            ToUserId = user.Id.ToString()
        };
        await DbContext.AddAsync(remind);
        await DbContext.SaveChangesAsync();

        var suggestions = await Service.GetRemindSuggestionsAsync(user);
        Assert.AreEqual(1, suggestions.Count);
    }
}
