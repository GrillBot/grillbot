using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class GuildUserTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildUser());
        }

        [TestMethod]
        public void Constructor_FromEntity()
        {
            var entity = new GrillBot.Database.Entity.GuildUser()
            {
                UsedInvite = new() { Creator = new() { User = new() } },
                User = new()
            };

            var user = new GuildUser(entity);
            Assert.IsNotNull(user.UsedInvite);
        }
    }
}
