using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class AutoReplyServiceTests
    {
        private static ServiceProvider CreateService(out AutoReplyService service, Dictionary<string, string> inMemoryCollection = null)
        {
            service = null;
            var container = DIHelpers.CreateContainer();

            if (container.GetService<GrillBotContextFactory>() is not TestingGrillBotContextFactory dbFactory)
            {
                Assert.Fail("DbFactory není TestingGrillBotContextFactory.");
                return null;
            }

            var configuration = ConfigHelpers.CreateConfiguration(inMemoryCollection: inMemoryCollection);
            service = new AutoReplyService(configuration, new DiscordSocketClient(), dbFactory, null);
            return container;
        }

        [TestMethod]
        public void Constructor_WithoutDisabledChannels()
        {
            using var _ = CreateService(out var service);
            Assert.IsNotNull(_);
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithDisabledChannels()
        {
            using var _ = CreateService(out var service, new Dictionary<string, string>()
            {
                { "AutoReply:DisabledChannels:0", "123456" }
            });
            Assert.IsNotNull(_);
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Init()
        {
            using var container = CreateService(out var service);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem() { Reply = "Reply", Template = "Template" });
            dbContext.SaveChanges();

            service.InitAsync().Wait();
        }
    }
}
