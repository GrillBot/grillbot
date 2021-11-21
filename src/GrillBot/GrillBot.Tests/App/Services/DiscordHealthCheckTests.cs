using Discord.WebSocket;
using GrillBot.App.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class DiscordHealthCheckTests
    {
        [TestMethod]
        public void CheckHeatlth_Disconnected()
        {
            var client = new DiscordSocketClient();
            var check = new DiscordHealthCheck(client);

            var result = check.CheckHealthAsync(null, CancellationToken.None).Result;
            Assert.AreEqual(HealthCheckResult.Unhealthy().Status, result.Status);
        }
    }
}
