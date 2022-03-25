using Discord;
using GrillBot.App.Services.Birthday;
using System;

namespace GrillBot.Tests.App.Services.Birthday;

[TestClass]
public class BirthdayHelperTests
{
    [TestMethod]
    public void Format_NoUsers()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var result = BirthdayHelper.Format(new(), configuration);

        Assert.AreEqual("Dnes nemá nikdo narozeniny :sadge:", result);
    }

    [TestMethod]
    public void Format_WithMultipleUsers()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var users = new List<Tuple<IUser, int?>>()
        {
            new(DataHelper.CreateDiscordUser(), null),
            new(DataHelper.CreateDiscordUser(), 1),
            new(DataHelper.CreateDiscordUser(), 2),
        };

        var result = BirthdayHelper.Format(users, configuration);
        Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public void Format_OneUser()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var users = new List<Tuple<IUser, int?>>() { new(DataHelper.CreateDiscordUser(), null), };

        var result = BirthdayHelper.Format(users, configuration);
        Assert.IsFalse(string.IsNullOrEmpty(result));
    }
}
