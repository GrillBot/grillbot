using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogItem());
        }

        [TestMethod]
        public void Filled()
        {
            var item = new UnverifyLogItem()
            {
                Id = 12345,
                CreatedAt = DateTime.MaxValue,
                FromUser = new(),
                Guild = new(),
                Operation = UnverifyOperation.Recover,
                RemoveData = new(),
                SetData = new(),
                ToUser = new(),
                UpdateData = new()
            };

            TestHelpers.CheckNonDefaultPropertyValues(item);
        }

        [TestMethod]
        public void ConstructorWithParameters_RemoveOperations()
        {
            var logItem = new GrillBot.Database.Entity.UnverifyLog()
            {
                CreatedAt = DateTime.MaxValue,
                Data = "{\"ReturnedRoles\":[], \"ReturnedOverwrites\":[]}",
                FromUser = new() { User = new() },
                Guild = new(),
                ToUser = new() { User = new() },
                Id = 12345,
                Operation = UnverifyOperation.Autoremove
            };

            var item = new UnverifyLogItem(logItem, null);
            Assert.IsNotNull(item.RemoveData);
            Assert.IsNull(item.SetData);
            Assert.IsNull(item.UpdateData);
        }

        [TestMethod]
        public void ConstructorWithParameters_UnverifyOperations()
        {
            var logItem = new GrillBot.Database.Entity.UnverifyLog()
            {
                CreatedAt = DateTime.MaxValue,
                Data = "{\"RolesToKeep\":[], \"RolesToRemove\":[],\"ChannelsToKeep\": [], \"ChannelsToRemove\":[]}",
                FromUser = new() { User = new() },
                Guild = new(),
                ToUser = new() { User = new() },
                Id = 12345,
                Operation = UnverifyOperation.Unverify
            };

            var item = new UnverifyLogItem(logItem, null);
            Assert.IsNull(item.RemoveData);
            Assert.IsNotNull(item.SetData);
            Assert.IsNull(item.UpdateData);
        }

        [TestMethod]
        public void ConstructorWithParameters_UpdateOperations()
        {
            var logItem = new GrillBot.Database.Entity.UnverifyLog()
            {
                CreatedAt = DateTime.MaxValue,
                Data = "{}",
                FromUser = new() { User = new() },
                Guild = new(),
                ToUser = new() { User = new() },
                Id = 12345,
                Operation = UnverifyOperation.Update
            };

            var item = new UnverifyLogItem(logItem, null);
            Assert.IsNull(item.RemoveData);
            Assert.IsNull(item.SetData);
            Assert.IsNotNull(item.UpdateData);
        }
    }
}
