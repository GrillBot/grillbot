using Discord.WebSocket;
using GrillBot.Data.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.Data.Models.API.AuditLog
{
    [TestClass]
    public class AuditLogListItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new AuditLogListItem());
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
                Type = AuditLogItemType.Info,
                GuildChannel = new GuildChannel(),
                Data = "13",
                Files = new HashSet<AuditLogFileMeta>() { new AuditLogFileMeta() }
            };

            var item = new AuditLogListItem(entity, new Newtonsoft.Json.JsonSerializerSettings());
            TestHelpers.CheckNonDefaultPropertyValues(item);
        }

        [TestMethod]
        public void BasicConstructor_OnlyUser()
        {
            var entity = new AuditLogItem()
            {
                Id = 50,
                CreatedAt = DateTime.Now,
                Guild = new Guild(),
                DiscordAuditLogItemId = "1234",
                Type = AuditLogItemType.Info,
                GuildChannel = new GuildChannel(),
                Data = "13",
                Files = new HashSet<AuditLogFileMeta>() { new AuditLogFileMeta() },
                ProcessedUser = new User()
            };

            var item = new AuditLogListItem(entity, new Newtonsoft.Json.JsonSerializerSettings());
            TestHelpers.CheckNonDefaultPropertyValues(item);
        }

        [TestMethod]
        public void BasicConstructor_WithoutOptional()
        {
            var item = new AuditLogListItem(new AuditLogItem(), new Newtonsoft.Json.JsonSerializerSettings());

            Assert.IsNull(item.Guild);
            Assert.IsNull(item.ProcessedUser);
            Assert.IsNull(item.Channel);
            Assert.AreEqual(0, item.Files.Count);
        }

        [TestMethod]
        public void Deserialization_Data()
        {
            var types = Enum.GetValues<AuditLogItemType>()
                .Where(o => o != AuditLogItemType.None);

            var jsonSettings = AuditLogService.JsonSerializerSettings;
            foreach (var type in types)
            {
                var entity = new AuditLogItem()
                {
                    Id = 50,
                    CreatedAt = DateTime.Now,
                    Guild = new Guild(),
                    ProcessedGuildUser = new GuildUser() { User = new User() },
                    DiscordAuditLogItemId = "1234",
                    Type = type,
                    GuildChannel = new GuildChannel(),
                    Data = "{}",
                    Files = new HashSet<AuditLogFileMeta>() { new AuditLogFileMeta() }
                };

                var item = new AuditLogListItem(entity, jsonSettings);
                Assert.IsNotNull(item.Data);
            }
        }
    }
}
