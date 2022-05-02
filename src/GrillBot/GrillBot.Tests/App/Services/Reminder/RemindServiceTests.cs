using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Reminder;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Namotion.Reflection;
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
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var at = DateTime.Now.AddDays(1);

        await Service.CreateRemindAsync(user, user, at, "msg", 970428820521893889);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task CreateRemindAsync_AnotherUser()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator.Replace("1", "5")).Build();
        var at = DateTime.Now.AddDays(1);

        await Service.CreateRemindAsync(from, to, at, "msg", 970428820521893889);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetRemindersCountAsync()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var result = await Service.GetRemindersCountAsync(user);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetRemindersAsync()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var result = await Service.GetRemindersAsync(user, 0);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_NotFound()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Service.CopyAsync(42, user);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_SameUser()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var at = DateTime.Now.AddDays(1);
        var id = await Service.CreateRemindAsync(user, user, at, "msg", 970428820521893889);
        await Service.CopyAsync(id, user);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_Finished()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();
        await DbContext.InitUserAsync(to, CancellationToken.None);
        await DbContext.InitUserAsync(from, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.MinValue,
            FromUserId = from.Id.ToString(),
            ToUserId = to.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 5
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(5, to);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_UserNotFound()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();
        var middle = new UserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(to);
        await DbContext.InitUserAsync(from);
        await DbContext.InitUserAsync(middle);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = from.Id.ToString(),
            ToUserId = middle.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(6, to);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task CopyAsync_MultipleSame()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(to);
        await DbContext.InitUserAsync(from);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = from.Id.ToString(),
            ToUserId = to.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 6
        });

        await DbContext.SaveChangesAsync();
        await Service.CopyAsync(6, to);
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
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var middle = new UserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(from, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.MinValue,
            FromUserId = from.Id.ToString(),
            ToUserId = from.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, middle, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_Finished2()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var middle = new UserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(from, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = from.Id.ToString(),
            ToUserId = from.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1,
            RemindMessageId = "12345"
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, middle, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CancelRemindAsync_NoPerms()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var middle = new UserBuilder().SetIdentity(Consts.UserId + 2, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(from, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = from.Id.ToString(),
            ToUserId = from.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();

        await Service.CancelRemindAsync(1, middle, false);
    }

    [TestMethod]
    public async Task CancelRemindAsync_Success()
    {
        var from = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var to = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username + "2", Consts.Discriminator).Build();

        await DbContext.InitUserAsync(to, CancellationToken.None);
        await DbContext.InitUserAsync(from, CancellationToken.None);
        await DbContext.Reminders.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now.AddDays(3),
            FromUserId = from.Id.ToString(),
            ToUserId = from.Id.ToString(),
            Message = "Message",
            OriginalMessageId = "12345",
            Id = 1
        });
        await DbContext.SaveChangesAsync();
        await Service.CancelRemindAsync(1, from, false);
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
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

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
