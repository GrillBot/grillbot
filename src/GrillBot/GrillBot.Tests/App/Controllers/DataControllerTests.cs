using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.Data.Controllers;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class DataControllerTests
    {
        private static ServiceProvider CreateController(string role, out DataController controller)
        {
            var client = new DiscordSocketClient();
            var commandService = new CommandService();
            var interactionService = new InteractionService(client);
            var container = DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

            controller = new DataController(client, dbContext, commandService, null, interactionService)
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

        private bool IsOk<TData>(ActionResult<TData> result)
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            return true;
        }

        [TestMethod]
        public void GetAvailableGuilds_User()
        {
            using var _ = CreateController("User", out DataController controller);
            var result = controller.GetAvailableGuildsAsync().Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetAvailableGuilds_Admin()
        {
            using var _ = CreateController("Admin", out DataController controller);
            var result = controller.GetAvailableGuildsAsync().Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetChannels_User()
        {
            using var _ = CreateController("User", out DataController controller);
            var result = controller.GetChannelsAsync(null).Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetChannels_Admin()
        {
            using var _ = CreateController("Admin", out DataController controller);
            var result = controller.GetChannelsAsync(null).Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetChannels_GuildId()
        {
            using var _ = CreateController("Admin", out DataController controller);
            var result = controller.GetChannelsAsync(1).Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetRoles_User()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetRoles(null);

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetRoles_Admin()
        {
            using var _ = CreateController("Admin", out var controller);
            var result = controller.GetRoles(null);

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetRoles_Guild()
        {
            using var _ = CreateController("Admin", out var controller);
            var result = controller.GetRoles(1);

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetCommandList()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetCommandsList();

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetAvailableUsers_Bots()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetAvailableUsersAsync(true).Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetAvailableUsers_OnlyUsers()
        {
            using var _ = CreateController("User", out var controller);
            var result = controller.GetAvailableUsersAsync(false).Result;

            Assert.IsTrue(IsOk(result));
        }

        [TestMethod]
        public void GetAvailableUsers_Admin()
        {
            using var _ = CreateController("Admin", out var controller);
            var result = controller.GetAvailableUsersAsync(null).Result;

            Assert.IsTrue(IsOk(result));
        }
    }
}
