using Castle.DynamicProxy.Generators;
using Discord;
using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class UserDetailTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UserDetail());
        }

        [TestMethod]
        public void FilledConstructor_KnownUser()
        {
            var activeClients = new List<ClientType>() { ClientType.Desktop };

            var user = new Mock<IUser>();
            user.Setup(o => o.ActiveClients).Returns(activeClients.ToImmutableHashSet());

            var entity = new GrillBot.Database.Entity.User()
            {
                ApiToken = Guid.NewGuid(),
            };

            entity.Guilds.Add(new() { Guild = new() });
            entity.UsedEmotes.Add(new() { EmoteId = "<:rtzW:567039874452946961>" });

            var userDetail = new UserDetail(entity, user.Object);
            Assert.IsTrue(userDetail.HaveApiToken);
            Assert.IsNotNull(userDetail.ActiveClients);
        }

        [TestMethod]
        public void FilledConstructor_UnknownUser()
        {
            var entity = new GrillBot.Database.Entity.User();

            var userDetail = new UserDetail(entity, null);
            Assert.IsFalse(userDetail.HaveApiToken);
            Assert.IsNull(userDetail.ActiveClients);
        }

        [TestMethod]
        public void FilledConstructor_WithBirthday()
        {
            var entity = new GrillBot.Database.Entity.User() { Birthday = DateTime.Now };

            var userDetail = new UserDetail(entity, null);
            Assert.IsTrue(userDetail.HaveBirthday);
        }
    }
}
