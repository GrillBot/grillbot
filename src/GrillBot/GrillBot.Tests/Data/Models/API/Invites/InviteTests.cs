using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Invites
{
    [TestClass]
    public class InviteTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GrillBot.Data.Models.API.Invites.Invite());
        }
    }
}
