using GrillBot.Data.Models.API.Params;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Common
{
    [TestClass]
    public class PaginatedParamsTests
    {
        [TestMethod]
        public void SkipProperty()
        {
            var parameters = new PaginatedParams() { Page = 2, PageSize = 20 };
            Assert.AreEqual(20, parameters.Skip);
        }
    }
}
