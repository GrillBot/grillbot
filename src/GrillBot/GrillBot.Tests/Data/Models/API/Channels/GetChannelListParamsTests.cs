using Discord;
using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class GetChannelListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new GetChannelListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) => Assert.AreEqual(propertyName == "SortBy" ? "Name" : defaultValue, value));
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetChannelListParams()
            {
                ChannelTypes = new(),
                GuildId = "Guild",
                NameContains = "Name",
                SortDesc = true
            };

            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetChannelListParams();
            var query = context.Channels.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetChannelListParams()
            {
                ChannelTypes = new() { ChannelType.Category },
                GuildId = "Guild",
                NameContains = "Name",
                SortDesc = true,
            };

            var query = context.Channels.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "name", "type" };
            var parameters = new GetChannelListParams();
            var baseQuery = context.Channels.AsQueryable();

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
