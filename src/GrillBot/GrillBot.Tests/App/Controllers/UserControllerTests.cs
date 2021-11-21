using Discord.WebSocket;
using GrillBot.App.Controllers;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class UserControllerTests
    {
        private static ServiceProvider CreateController(string role, out UsersController controller)
        {
            var client = new DiscordSocketClient();
            var container = DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

            controller = new UsersController(dbContext, client)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Role, role),
                            new Claim(ClaimTypes.NameIdentifier, "1")
                        }))
                    }
                }
            };

            return container;
        }

        [TestMethod]
        public void GetCurrentUserDetail_NotFound()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetCurrentUserDetailAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetCurrentUserDetail_Ok()
        {
            using var container = CreateController("User", out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
            dbContext.Users.Add(new GrillBot.Database.Entity.User()
            {
                Id = "1"
            });
            dbContext.SaveChanges();

            var result = controller.GetCurrentUserDetailAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Hearthbeat()
        {
            using var container = CreateController("User", out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
            dbContext.Users.Add(new GrillBot.Database.Entity.User()
            {
                Id = "1"
            });
            dbContext.SaveChanges();

            var result = controller.HearthbeatAsync(true).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void HearthbeatOff()
        {
            using var container = CreateController("User", out var controller);
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.Users.RemoveRange(dbContext.Users.AsEnumerable());
            dbContext.Users.Add(new GrillBot.Database.Entity.User()
            {
                Id = "1"
            });
            dbContext.SaveChanges();

            var result = controller.HearthbeatOffAsync(true).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }
    }
}
