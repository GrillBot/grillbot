using GrillBot.Data.Models.API.Guilds;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Guilds
{
    [TestClass]
    public class UpdateGuildParamsTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new UpdateGuildParams());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            TestHelpers.CheckNonDefaultPropertyValues(new UpdateGuildParams()
            {
                AdminChannelId = "AdminChannel",
                MuteRoleId = "MuteRoleId"
            });
        }
    }
}
