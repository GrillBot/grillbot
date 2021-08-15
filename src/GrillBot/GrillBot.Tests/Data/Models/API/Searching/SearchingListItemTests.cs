using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Searching
{
    [TestClass]
    public class SearchingListItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new SearchingListItem(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var entity = new SearchItem()
            {
                Channel = new GuildChannel(),
                Guild = new Guild(),
                User = new User(),
                Id = 12345
            };

            var item = new SearchingListItem(entity, "Něco", "http://discord.com");
            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
            Assert.IsTrue(Uri.IsWellFormedUriString(item.JumpLink, UriKind.Absolute));
        }
    }
}
