using GrillBot.Data.Services.Discord;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Services.Discord
{
    [TestClass]
    public class DiscordInitializationServiceTests
    {
        [TestMethod]
        public void SetAndGet()
        {
            var logger = NullLogger<DiscordInitializationService>.Instance;
            var service = new DiscordInitializationService(logger);

            Assert.IsFalse(service.Get());
            service.Set(true);
            Assert.IsTrue(service.Get());
        }
    }
}
