using GrillBot.Data.Models.API.Guilds;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Guilds
{
    [TestClass]
    public class GetGuildListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            TestHelpers.CheckDefaultPropertyValues(new GetGuildListParams());
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetGuildListParams()
            {
                NameQuery = "Name"
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetGuildListParams();
            var query = context.Guilds.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetGuildListParams()
            {
                NameQuery = "Name"
            };

            var query = context.Guilds.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }
    }
}
