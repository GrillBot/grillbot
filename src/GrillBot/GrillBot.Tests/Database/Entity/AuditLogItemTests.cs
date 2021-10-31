using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class AuditLogItemTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new AuditLogItem(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "Files":
                        Assert.AreNotEqual(defaultValue, value);
                        break;
                    default:
                        Assert.AreEqual(defaultValue, value);
                        break;
                }
            });
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var item = new AuditLogItem()
            {
                ChannelId = "Channel",
                CreatedAt = DateTime.MaxValue,
                Data = "{}",
                DiscordAuditLogItemId = "Item",
                Guild = new(),
                GuildChannel = new(),
                GuildId = "Guild",
                Id = 50,
                ProcessedGuildUser = new(),
                ProcessedUserId = "User",
                Type = AuditLogItemType.ChannelCreated
            };

            TestHelpers.CheckNonDefaultPropertyValues(item);
        }

        [TestMethod]
        public void Create()
        {
            new[]
            {
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, null, null, null, "{}"),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, null, null, null, "{}", 12345),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, null, null, null, "{}", "12345"),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, null, null, null, "{}", null as ulong?),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, new Mock<IGuild>().Object, new Mock<IChannel>().Object, null, "{}", null as ulong?),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, new Mock<IGuild>().Object, null, null, "{}", null as ulong?),
                AuditLogItem.Create(AuditLogItemType.ChannelCreated, new Mock<IGuild>().Object, new Mock<IChannel>().Object, new Mock<IUser>().Object, "{}", null as ulong?)
            }.ToList().ForEach(o => Assert.IsNotNull(o));
        }
    }
}
