using Discord;
using GrillBot.App.Services.Birthday;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Services.Birthday
{
    [TestClass]
    public class BirthdayHelperTests
    {
        [TestMethod]
        public void Format_NoUsersHaveBirthday()
        {
            var users = new List<Tuple<IUser, int?>>();
            var config = ConfigHelpers.CreateConfiguration();
            const string expected = "Dnes nemá nikdo narozeniny <sadge>";

            var result = BirthdayHelper.Format(users, config);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Format_WithoutYears()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Username).Returns("Username");

            var users = new List<Tuple<IUser, int?>>()
            {
                new(user.Object, null)
            };

            var config = ConfigHelpers.CreateConfiguration();
            var result = BirthdayHelper.Format(users, config);

            Assert.AreEqual("Dnes má narozeniny **Username** <hypers>", result);
        }

        [TestMethod]
        public void Format_WithYears()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Username).Returns("Username");

            var users = new List<Tuple<IUser, int?>>()
            {
                new(user.Object, 24)
            };

            var config = ConfigHelpers.CreateConfiguration();
            var result = BirthdayHelper.Format(users, config);

            Assert.AreEqual("Dnes má narozeniny **Username (24 let)** <hypers>", result);
        }

        [TestMethod]
        public void Format_MultipleUsers()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Username).Returns("Username");

            var users = new List<Tuple<IUser, int?>>()
            {
                new(user.Object, 24),
                new(user.Object, 23)
            };

            var config = ConfigHelpers.CreateConfiguration();
            var result = BirthdayHelper.Format(users, config);

            Assert.AreEqual("Dnes mají narozeniny **Username (24 let)** a **Username (23 let)** <hypers>", result);
        }

        [TestMethod]
        public void Format_MultipleWithoutYears()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Username).Returns("Username");

            var users = new List<Tuple<IUser, int?>>()
            {
                new(user.Object, null),
                new(user.Object, 23)
            };

            var config = ConfigHelpers.CreateConfiguration();
            var result = BirthdayHelper.Format(users, config);

            Assert.AreEqual("Dnes mají narozeniny **Username** a **Username (23 let)** <hypers>", result);
        }
    }
}
