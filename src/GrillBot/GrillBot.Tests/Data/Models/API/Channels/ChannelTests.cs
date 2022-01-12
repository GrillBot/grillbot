using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class ChannelTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new Channel());
        }

        [TestMethod]
        public void FilledConstructor_EmptyDates()
        {
            var entity = new GrillBot.Database.Entity.GuildChannel();
            entity.Users.Add(new GrillBot.Database.Entity.GuildUserChannel() { Count = 5 });
            var channel = new Channel(entity);

            Assert.IsNull(channel.FirstMessageAt);
            Assert.IsNull(channel.LastMessageAt);
        }
    }
}
