using Discord;
using GrillBot.Data.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Npgsql.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class GuildExtensionsTests
    {
        [TestMethod]
        public void CalculateFileUploadLimit()
        {
            var tiers = new[]
            {
                Tuple.Create(PremiumTier.None, 8),
                Tuple.Create(PremiumTier.Tier1, 8),
                Tuple.Create(PremiumTier.Tier2, 50),
                Tuple.Create(PremiumTier.Tier3, 100)
            };

            foreach (var tier in tiers)
            {
                var guild = new Mock<IGuild>();
                guild.Setup(o => o.PremiumTier).Returns(tier.Item1);

                Assert.AreEqual(tier.Item2, guild.Object.CalculateFileUploadLimit());
                guild.Verify(o => o.PremiumTier, Times.Once());
            }
        }

        [TestMethod]
        public void GetHighestRole_WithoutColor()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Name).Returns("A");
            role.Setup(o => o.Position).Returns(2);
            role.Setup(o => o.Color).Returns(Color.Default);

            var anotherRole = new Mock<IRole>();
            anotherRole.Setup(o => o.Name).Returns("B");
            anotherRole.Setup(o => o.Position).Returns(1);
            anotherRole.Setup(o => o.Color).Returns(Color.Default);

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Roles).Returns(new[] { role.Object, anotherRole.Object });

            var selectedRole = guild.Object.GetHighestRole();
            Assert.AreEqual("A", selectedRole.Name);
        }

        [TestMethod]
        public void GetHighestRole_WithColor()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Name).Returns("A");
            role.Setup(o => o.Position).Returns(2);
            role.Setup(o => o.Color).Returns(Color.Default);

            var anotherRole = new Mock<IRole>();
            anotherRole.Setup(o => o.Name).Returns("B");
            anotherRole.Setup(o => o.Position).Returns(1);
            anotherRole.Setup(o => o.Color).Returns(Color.Blue);

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Roles).Returns(new[] { role.Object, anotherRole.Object });

            var selectedRole = guild.Object.GetHighestRole(true);
            Assert.AreEqual("B", selectedRole.Name);
        }
    }
}
