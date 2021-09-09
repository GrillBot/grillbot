using GrillBot.Data.Models.API.Searching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Searching
{
    [TestClass]
    public class GetSearchingListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new GetSearchingListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) => Assert.AreEqual(propertyName == "SortBy" ? "Id" : defaultValue, value));
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetSearchingListParams()
            {
                ChannelId = "",
                GuildId = "",
                SortDesc = true,
                UserId = ""
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetSearchingListParams();
            var query = context.SearchItems.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetSearchingListParams()
            {
                ChannelId = "1",
                GuildId = "1",
                SortDesc = true,
                UserId = "1"
            };

            var query = context.SearchItems.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "id", "user", "guild", "channel" };
            var parameters = new GetSearchingListParams();
            var baseQuery = context.SearchItems.AsQueryable();

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
