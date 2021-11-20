using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models.API.System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace GrillBot.Tests.Data.Models.API.System
{
    [TestClass]
    public class DiagnosticsInfoTests
    {
        [TestMethod]
        public void Empty()
        {
            Thread.Sleep(100);
            var info = new DiagnosticsInfo();

            Assert.AreNotEqual(DateTime.MinValue, info.StartAt);
            Assert.IsTrue(info.Uptime.TotalMilliseconds > 0);
            Assert.IsTrue(info.UsedMemory > 0);
            Assert.AreEqual(UserStatus.Offline, info.UserStatus);
            Assert.IsTrue(info.CpuTime.TotalMilliseconds >= 0);
            Assert.AreNotEqual(DateTime.MinValue, info.CurrentDateTime);
        }

        [TestMethod]
        public void Filled()
        {
            using var client = new DiscordSocketClient();
            var info = new DiagnosticsInfo("Release", client);

            Assert.AreEqual("Release", info.InstanceType);
            Assert.AreEqual(0, info.Latency.TotalMilliseconds);
            Assert.AreEqual(ConnectionState.Disconnected, info.ConnectionState);
        }
    }
}
