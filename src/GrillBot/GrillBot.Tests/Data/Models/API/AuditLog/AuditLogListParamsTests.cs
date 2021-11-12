using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.AuditLog
{
    [TestClass]
    public class AuditLogListParamsTests
    {
        [TestMethod]
        public void Empty()
        {
            var parameters = new AuditLogListParams();
            TestHelpers.CheckDefaultPropertyValues(parameters, (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "SortBy":
                        Assert.AreEqual("CreatedAt", value);
                        break;
                    case "SortDesc":
                        Assert.IsTrue((bool)value);
                        break;
                    case "Types":
                        Assert.IsNotNull(value);
                        break;
                    default:
                        Assert.AreEqual(defaultValue, value);
                        break;
                }
            });
        }

        [TestMethod]
        public void Filled()
        {
            var parameters = new AuditLogListParams()
            {
                ChannelId = "1",
                CreatedFrom = DateTime.MinValue,
                CreatedTo = DateTime.MaxValue,
                GuildId = "1",
                IgnoreBots = true,
                ProcessedUserIds = new() { "1" },
                SortBy = "CreatedAt",
                SortDesc = true,
                Types = new List<AuditLogItemType>()
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_EmptyFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new AuditLogListParams();
            var query = context.AuditLogs.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_DefaultSort_FilledFilter()
        {
            using var context = TestHelpers.CreateDbContext();

            var parameters = new AuditLogListParams()
            {
                ChannelId = "1",
                CreatedFrom = DateTime.MinValue,
                CreatedTo = DateTime.MaxValue,
                GuildId = "1",
                IgnoreBots = true,
                ProcessedUserIds = new() { "1" },
                SortBy = "CreatedAt",
                SortDesc = true,
                Types = new List<AuditLogItemType>() { AuditLogItemType.ChannelCreated }
            };

            var query = context.AuditLogs.AsQueryable();
            query = parameters.CreateQuery(query);

            Assert.IsNotNull(query);
        }

        [TestMethod]
        public void CreateQuery_Sorts()
        {
            using var context = TestHelpers.CreateDbContext();
            var cases = new List<string>() { "guild", "processed", "type", "channel" };
            var parameters = new AuditLogListParams();
            var baseQuery = context.AuditLogs.AsQueryable();

            foreach (var @case in cases)
            {
                parameters.SortDesc = false;
                parameters.SortBy = @case;
                Assert.IsNotNull(parameters.CreateQuery(baseQuery));

                parameters.SortDesc = true;
                Assert.IsNotNull(parameters.CreateQuery(baseQuery));
            }
        }
    }
}
