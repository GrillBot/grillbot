using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new UnverifyLogParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) => Assert.AreEqual(propertyName == "SortBy" ? "CreatedAt" : defaultValue, value));
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new UnverifyLogParams()
            {
                CreatedFrom = DateTime.MinValue,
                CreatedTo = DateTime.MaxValue,
                FromUserId = "1",
                GuildId = "",
                Operation = UnverifyOperation.Autoremove,
                SortDesc = true,
                SortBy = "",
                ToUserId = ""
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new UnverifyLogParams();
            var query = context.UnverifyLogs.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new UnverifyLogParams()
            {
                CreatedFrom = DateTime.MinValue,
                CreatedTo = DateTime.MaxValue,
                FromUserId = "1",
                GuildId = "Guild",
                Operation = UnverifyOperation.Unverify,
                SortDesc = true,
                SortBy = "",
                ToUserId = "User"
            };

            var query = context.UnverifyLogs.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "operation", "guild", "fromuser", "touser" };
            var parameters = new UnverifyLogParams();
            var baseQuery = context.UnverifyLogs.AsQueryable();

            foreach (var @case in cases)
            {
                parameters.SortDesc = false;
                parameters.SortBy = @case;
                Assert.IsNotNull(parameters.CreateQuery(baseQuery));

                parameters.SortDesc = true;
                Assert.IsNotNull(parameters.CreateQuery(baseQuery));
            }
        }
    }
}
