using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }

        [TestMethod]
        public void Create()
        {
            var item = AuditLogItem.Create(AuditLogItemType.ChannelCreated, null, null, null, "{}", 12345);
            Assert.IsNotNull(item);
        }
    }
}
