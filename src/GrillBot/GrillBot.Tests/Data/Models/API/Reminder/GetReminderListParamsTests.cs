using GrillBot.Data.Models.API.Reminder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Reminder
{
    [TestClass]
    public class GetReminderListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new GetReminderListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) => Assert.AreEqual(propertyName == "SortBy" ? "Id" : defaultValue, value));
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetReminderListParams()
            {
                CreatedFrom = DateTime.MaxValue,
                CreatedTo = DateTime.MaxValue,
                FromUserId = "User",
                MessageContains = "Message",
                OriginalMessageId = "Id",
                SortBy = "Id",
                SortDesc = true,
                ToUserId = "Id"
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetReminderListParams();
            var query = context.Reminders.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetReminderListParams()
            {
                CreatedFrom = DateTime.MaxValue,
                CreatedTo = DateTime.MaxValue,
                FromUserId = "User",
                MessageContains = "Message",
                OriginalMessageId = "Id",
                SortBy = "Id",
                SortDesc = true,
                ToUserId = "Id"
            };

            var query = context.Reminders.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "Id", "FromUser", "ToUser", "At", "Postpone" };
            var parameters = new GetReminderListParams();
            var baseQuery = context.Reminders.AsQueryable();

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
