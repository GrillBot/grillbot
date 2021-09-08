using GrillBot.Data.Models.API.Guilds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Guilds
{
    [TestClass]
    public class GuildDetailTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildDetail());
        }

        [TestMethod]
        public void FilledConstructor_WithoutGuild()
        {
            var entity = new GrillBot.Database.Entity.Guild() { Name = "Name" };
            var detail = new GuildDetail(null, entity);

            Assert.AreEqual("Name", detail.Name);
        }

        [TestMethod]
        public void Filled()
        {
            var detail = new GuildDetail()
            {
                CreatedAt = DateTimeOffset.MaxValue,
                IconUrl = "Icon",
                IsConnected = true,
                Owner = new(),
                PremiumTier = Discord.PremiumTier.Tier1,
                VanityUrl = "Vanity",
                MutedRole = new(),
                BoosterRole = new(),
                AdminChannel = new()
            };

            TestHelpers.CheckNonDefaultPropertyValues(detail);
        }
    }
}
