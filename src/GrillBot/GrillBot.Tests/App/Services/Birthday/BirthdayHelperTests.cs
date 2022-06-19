using Discord;
using GrillBot.App.Services.Birthday;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;

namespace GrillBot.Tests.App.Services.Birthday;

[TestClass]
public class BirthdayHelperTests
{
    [TestMethod]
    public void Format_NoUsers()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var result = BirthdayHelper.Format(new List<(IUser user, int? age)>(), configuration);

        Assert.AreEqual("Dnes nemá nikdo narozeniny :sadge:", result);
    }

    [TestMethod]
    public void Format_WithMultipleUsers()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var configuration = ConfigurationHelper.CreateConfiguration();
        var users = new List<(IUser user, int? age)> { new(user, null), new(user, 1), new(user, 2) };

        var result = BirthdayHelper.Format(users, configuration);
        Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public void Format_OneUser()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var users = new List<(IUser user, int? age)> { new(user, null), };

        var result = BirthdayHelper.Format(users, configuration);
        Assert.IsFalse(string.IsNullOrEmpty(result));
    }
}
