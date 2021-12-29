using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class AutoReplyServiceTests
    {
        private ServiceProvider CreateService(out AutoReplyService service, Dictionary<string, string> inMemoryCollection = null)
        {
            service = null;
            var container = DIHelpers.CreateContainer();

            if (container.GetService<GrillBotContextFactory>() is not TestingGrillBotContextFactory dbFactory)
            {
                Assert.Fail("DbFactory není TestingGrillBotContextFactory.");
                return null;
            }

            var configuration = TestHelper.ConfigHelpers.CreateConfiguration(inMemoryCollection: inMemoryCollection);
            service = new AutoReplyService(configuration, new DiscordSocketClient(), dbFactory);
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
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem());
            dbContext.SaveChanges();

            service.InitAsync().Wait();
        }
    }
}
