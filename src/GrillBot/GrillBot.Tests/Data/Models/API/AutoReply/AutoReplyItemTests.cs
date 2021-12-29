using GrillBot.Data.Models.API.AutoReply;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.AutoReply
{
    [TestClass]
    public class AutoReplyItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new AutoReplyItem());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var entity = new GrillBot.Database.Entity.AutoReplyItem()
            {
                Flags = 3,
                Id = 1,
                Reply = "Test",
                Template = "Template"
            };

            var item = new AutoReplyItem(entity);
            TestHelpers.CheckNonDefaultPropertyValues(item);
        }
    }
}
