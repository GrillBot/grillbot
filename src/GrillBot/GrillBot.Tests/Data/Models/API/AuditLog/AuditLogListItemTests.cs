using Discord.WebSocket;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
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
            TestHelpers.CheckDefaultPropertyValues(item);
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
                Type = GrillBot.Database.Enums.AuditLogItemType.Info,
                GuildChannel = new GuildChannel(),
                Data = "13",
                Files = new HashSet<AuditLogFileMeta>() { new AuditLogFileMeta() }
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
            var types = new[]
            {
                AuditLogItemType.Command,
                AuditLogItemType.ChannelCreated,
                AuditLogItemType.ChannelDeleted,
                AuditLogItemType.ChannelUpdated,
                AuditLogItemType.EmojiDeleted,
                AuditLogItemType.GuildUpdated,
                AuditLogItemType.MemberRoleUpdated,
                AuditLogItemType.MemberUpdated,
                AuditLogItemType.MessageDeleted,
                AuditLogItemType.MessageEdited,
                AuditLogItemType.OverwriteCreated,
                AuditLogItemType.OverwriteDeleted,
                AuditLogItemType.OverwriteUpdated,
                AuditLogItemType.Unban,
                AuditLogItemType.UserJoined,
                AuditLogItemType.UserLeft
            };

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
