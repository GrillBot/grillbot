using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Security.Claims;

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
            var commandService = new CommandService();
            var initializationService = new DiscordInitializationService(NullLogger<DiscordInitializationService>.Instance);
            var cache = new MessageCache(client, initializationService);
            var configuration = ConfigHelpers.CreateConfiguration();
            var channelService = new ChannelService(client, null, configuration, cache);
            var helpService = new HelpService(client, commandService, channelService, container, configuration);

            controller = new UsersController(dbContext, client, helpService)
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
                Id = "1",
                Username = "Username"
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
                Id = "1",
                Username = "Username"
            });
            dbContext.SaveChanges();

            var result = controller.HearthbeatAsync().Result;

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
                Id = "1",
                Username = "Username"
            });
            dbContext.SaveChanges();

            var result = controller.HearthbeatOffAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void GetAvailableCommands()
        {
            using var _ = CreateController("User", out var controller);

            var result = controller.GetAvailableCommandsAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }
    }
}
