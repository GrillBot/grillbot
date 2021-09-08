using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class UpdateUserParamsTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new UpdateUserParams(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            var parameters = new UpdateUserParams()
            {
                ApiToken = Guid.NewGuid(),
                BotAdmin = true,
                Note = "Note"
            };

            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
