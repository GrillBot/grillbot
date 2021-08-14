using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.AuditLog
{
    [TestClass]
    public class AuditLogListItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            var item = new AuditLogListItem();
            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void BasicConstructor()
        {
            var entity = new AuditLogItem()
            {
                Id = 50,
                CreatedAt = DateTime.Now,
                Guild = new Guild(),
                ProcessedGuildUser = new GuildUser() { User = new User() },
                DiscordAuditLogItemId = "1234",
                Type = GrillBot.Database.Enums.AuditLogItemType.ChannelCreated,
                GuildChannel = new GuildChannel(),
                Data = "13",
                Files = new HashSet<AuditLogFileMeta>() { new AuditLogFileMeta() }
            };

            var item = new AuditLogListItem(entity);
            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }

        [TestMethod]
        public void BasicConstructor_WithoutOptional()
        {
            var item = new AuditLogListItem(new AuditLogItem());

            Assert.IsNull(item.Guild);
            Assert.IsNull(item.ProcessedUser);
            Assert.IsNull(item.Channel);
            Assert.AreEqual(0, item.Files.Count);
        }
    }
}
