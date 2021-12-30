using Discord.WebSocket;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.Data.Models.API.AutoReply;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class AutoReplyControllerTests
    {
        private static ServiceProvider CreateService(out AutoReplyService service)
        {
            service = null;
            var container = DIHelpers.CreateContainer();

            if (container.GetService<GrillBotContextFactory>() is not TestingGrillBotContextFactory dbFactory)
            {
                Assert.Fail("DbFactory není TestingGrillBotContextFactory.");
                return null;
            }

            var configuration = ConfigHelpers.CreateConfiguration();
            service = new AutoReplyService(configuration, new DiscordSocketClient(), dbFactory, null);
            return container;
        }

        private static ServiceProvider CreateController(out AutoReplyController controller)
        {
            var container = CreateService(out var service);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

            controller = new AutoReplyController(service, dbContext);
            return container;
        }

        [TestMethod]
        public void GetAutoReplyList()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem());
            dbContext.SaveChanges();

            var list = controller.GetAutoReplyListAsync().Result;
            Assert.IsNotNull(list);
            Assert.IsNotNull(list.Result);
            Assert.IsInstanceOfType(list.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetItem_NotFound()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.SaveChanges();

            var result = controller.GetItemAsync(1).Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetItem_Found()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem() { Id = 1 });
            dbContext.SaveChanges();

            var result = controller.GetItemAsync(1).Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void CreateItem()
        {
            using var _ = CreateController(out var controller);

            var parameters = new AutoReplyItemParams()
            {
                Flags = 1,
                Reply = "Reply",
                Template = "Template"
            };

            var result = controller.CreateItemAsync(parameters).Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void UpdateItem_NotFound()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.SaveChanges();

            var result = controller.UpdateItemAsync(1, null).Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void UpdateItem_Found()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem() { Id = 1 });
            dbContext.SaveChanges();

            var parameters = new AutoReplyItemParams()
            {
                Flags = 1,
                Reply = "Reply",
                Template = "Template"
            };

            var result = controller.UpdateItemAsync(1, parameters).Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void RemoveItem_NotFound()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.SaveChanges();

            var result = controller.RemoveItemAsync(1).Result;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void RemoveItem_Found()
        {
            using var container = CreateController(out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AutoReplies.RemoveRange(dbContext.AutoReplies.ToList());
            dbContext.AutoReplies.Add(new GrillBot.Database.Entity.AutoReplyItem() { Id = 1 });
            dbContext.SaveChanges();

            var result = controller.RemoveItemAsync(1).Result;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }
    }
}
