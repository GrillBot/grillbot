using GrillBot.Data.Models.API.AutoReply;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.AutoReply
{
    [TestClass]
    public class AutoReplyItemParamsTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new AutoReplyItemParams());
        }
    }
}
