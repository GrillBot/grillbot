using Discord.WebSocket;
using GrillBot.Data.Controllers;
using GrillBot.Data.Services.Unverify;
using GrillBot.Data.Models.API.Unverify;
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
    public class UnverifyControllerTests
    {
        private static ServiceProvider CreateController(string role, out UnverifyController controller)
        {
            var client = new DiscordSocketClient();
            var container = DIHelpers.CreateContainer();
            var dbFactory = (GrillBotContextFactory)container.GetService(typeof(GrillBotContextFactory));
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            var unverifyService = new UnverifyService(client, null, null, null, dbFactory, null);

            controller = new UnverifyController(unverifyService, client, dbContext)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Role, role)
                        }))
                    }
                }
            };

            return container;
        }

        [TestMethod]
        public void GetCurrentUnverifies_User()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetCurrentUnverifiesAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetCurrentUnverifies_Admin()
        {
            using var _ = CreateController("Admin", out var controller);
            var result = controller.GetCurrentUnverifiesAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetUnverifyLog_User()
        {
            using var _ = CreateController("User", out var controller);
            var @params = new UnverifyLogParams();
            var result = controller.GetUnverifyLogsAsync(@params).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetUnverifyLog_Admin()
        {
            using var _ = CreateController("Admin", out var controller);
            var @params = new UnverifyLogParams();
            var result = controller.GetUnverifyLogsAsync(@params).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }
    }
}
