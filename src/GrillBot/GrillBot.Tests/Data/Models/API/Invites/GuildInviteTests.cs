using GrillBot.Data.Models.API.Invites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Invites
{
    [TestClass]
    public class GuildInviteTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildInvite());
        }

        [TestMethod]
        public void WithoutGuild()
        {
            var entity = new GrillBot.Database.Entity.Invite() { Creator = new() { User = new() } };
            var invite = new GuildInvite(entity);

            Assert.IsNull(invite.Guild);
        }

        [TestMethod]
        public void WithGuild()
        {
            var entity = new GrillBot.Database.Entity.Invite() { Guild = new(), Creator = new() { User = new() } };
            var invite = new GuildInvite(entity);

            Assert.IsNotNull(invite.Guild);
        }
    }
}
