using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void BirthdayAcceptYear_True()
        {
            var user = new User() { Birthday = new DateTime(1996, 4, 16) };
            Assert.IsTrue(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void BirthdayAcceptYear_MissingBirthday_False()
        {
            var user = new User();
            Assert.IsFalse(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void BirthdayAcceptYear_WithBirthday_False()
        {
            var user = new User() { Birthday = new DateTime(1, 6, 3) };
            Assert.IsFalse(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void HaveFlags_Single_True()
        {
            var user = new User() { Flags = (int)UserFlags.BotAdmin };

            Assert.IsTrue(user.HaveFlags(UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Multiple_True()
        {
            var user = new User() { Flags = (int)(UserFlags.BotAdmin | UserFlags.WebAdmin) };

            Assert.IsTrue(user.HaveFlags(UserFlags.WebAdmin | UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Single_False()
        {
            var user = new User();

            Assert.IsFalse(user.HaveFlags(UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Multiple_False()
        {
            var user = new User();

            Assert.IsFalse(user.HaveFlags(UserFlags.BotAdmin | UserFlags.WebAdmin));
        }
    }
}
