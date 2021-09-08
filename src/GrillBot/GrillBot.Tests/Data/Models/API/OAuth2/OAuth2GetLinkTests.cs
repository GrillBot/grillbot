using GrillBot.Data.Models.API.OAuth2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.OAuth2
{
    [TestClass]
    public class OAuth2GetLinkTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new OAuth2GetLink());
        }
    }
}
