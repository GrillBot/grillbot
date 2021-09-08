using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class GuildChannelTests
    {
        [TestMethod]
        public void Constructor_WithoutGuild()
        {
            var entity = new GrillBot.Database.Entity.GuildChannel();
            var channel = new GuildChannel(entity);
            Assert.IsNull(channel.Guild);
        }
    }
}
