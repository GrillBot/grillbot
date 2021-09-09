using GrillBot.Data.Models.API.Invites;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Invites
{
    [TestClass]
    public class GetInviteListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new GetInviteListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) => Assert.AreEqual(propertyName == "SortBy" ? "Code" : defaultValue, value));
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetInviteListParams()
            {
                Code = "Code",
                CreatedFrom = DateTime.MaxValue,
                CreatedTo = DateTime.MaxValue,
                CreatorId = "Creator",
                GuildId = "Guild",
                SortDesc = true
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetInviteListParams();
            var query = context.Invites.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetInviteListParams()
            {
                Code = "Code",
                CreatedFrom = DateTime.MaxValue,
                CreatedTo = DateTime.MaxValue,
                CreatorId = "Creator",
                GuildId = "Guild",
                SortDesc = true
            };

            var query = context.Invites.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "code", "createdat", "creator" };
            var parameters = new GetInviteListParams();
            var baseQuery = context.Invites.AsQueryable();

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
