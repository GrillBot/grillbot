using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class GetUserListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new GetUserListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new GetUserListParams()
            {
                Flags = 42,
                GuildId = "Guild",
                HaveApiAccess = true,
                HaveBirthday = true,
                SortDesc = true,
                Username = "Username"
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetUserListParams();
            var query = context.Users.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new GetUserListParams()
            {
                Flags = 42,
                GuildId = "Guild",
                HaveApiAccess = true,
                HaveBirthday = true,
                SortDesc = true,
                Username = "Username"
            };

            var query = context.Users.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }
    }
}
