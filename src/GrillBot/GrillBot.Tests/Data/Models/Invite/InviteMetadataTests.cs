using Discord;
using GrillBot.Data.Models.Invite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.Data.Models.Invite
{
    [TestClass]
    public class InviteMetadataTests
    {
        [TestMethod]
        public void FromDiscord()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(100);

            var inviter = new Mock<IUser>();
            inviter.Setup(o => o.Id).Returns(1);

            var metadata = new Mock<IInviteMetadata>();
            metadata.Setup(o => o.Uses).Returns(50);
            metadata.Setup(o => o.Guild).Returns(guild.Object);
            metadata.Setup(o => o.Code).Returns("Code");
            metadata.Setup(o => o.Inviter).Returns(inviter.Object);
            metadata.Setup(o => o.CreatedAt).Returns(DateTimeOffset.MaxValue);

            var inviteMetadata = InviteMetadata.FromDiscord(metadata.Object);
            var entity = inviteMetadata.ToEntity();

            Assert.AreEqual(inviteMetadata.CreatedAt, entity.CreatedAt);
            Assert.AreEqual(inviteMetadata.Code, entity.Code);
            Assert.AreEqual(inviteMetadata.CreatorId.ToString(), entity.CreatorId);
            Assert.AreEqual(inviteMetadata.GuildId.ToString(), entity.GuildId);
            Assert.AreEqual(50, inviteMetadata.Uses);
            Assert.IsFalse(inviteMetadata.IsVanity);
        }

        [TestMethod]
        public void FromDiscord_Vanity()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(100);
            guild.Setup(o => o.VanityURLCode).Returns("Code");

            var metadata = new Mock<IInviteMetadata>();
            metadata.Setup(o => o.Uses).Returns(50);
            metadata.Setup(o => o.Guild).Returns(guild.Object);
            metadata.Setup(o => o.Code).Returns("Code");

            var inviteMetadata = InviteMetadata.FromDiscord(metadata.Object);
            var entity = inviteMetadata.ToEntity();

            Assert.AreEqual(inviteMetadata.CreatedAt, entity.CreatedAt);
            Assert.AreEqual(inviteMetadata.Code, entity.Code);
            Assert.AreEqual(inviteMetadata.CreatorId, entity.CreatorId);
            Assert.AreEqual(inviteMetadata.GuildId.ToString(), entity.GuildId);
            Assert.AreEqual(50, inviteMetadata.Uses);
            Assert.IsTrue(inviteMetadata.IsVanity);
        }
    }
}
