using Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.Unverify
{
    [TestClass]
    public class UnverifyLogSetTests
    {
        [TestMethod]
        public void FromProfile()
        {
            var user = new Mock<IGuildUser>();
            var role = new Mock<IRole>();

            var profile = new UnverifyUserProfile(user.Object, DateTime.MinValue, DateTime.MaxValue, false)
            {
                ChannelsToKeep = new List<ChannelOverride>() { new ChannelOverride(42, 0, 0) },
                ChannelsToRemove = new List<ChannelOverride>(),
                Reason = "Duvod",
                RolesToKeep = new List<IRole>() { role.Object },
                RolesToRemove = new List<IRole>() { role.Object }
            };

            var item = UnverifyLogSet.FromProfile(profile);

            Assert.AreEqual(profile.ChannelsToKeep.Count, item.ChannelsToKeep.Count);
            Assert.AreEqual(profile.ChannelsToRemove.Count, item.ChannelsToRemove.Count);
            Assert.AreEqual(profile.Start, item.Start);
            Assert.AreEqual(profile.End, item.End);
            Assert.AreEqual(profile.Reason, item.Reason);
            Assert.AreEqual(profile.RolesToKeep.Count, item.RolesToKeep.Count);
            Assert.AreEqual(profile.RolesToRemove.Count, item.RolesToRemove.Count);
            Assert.AreEqual(profile.IsSelfUnverify, item.IsSelfUnverify);
        }
    }
}
