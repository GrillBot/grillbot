using GrillBot.Data.Models.API.Invites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Invites
{
    [TestClass]
    public class InviteBaseTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new InviteBase());
        }
    }
}
