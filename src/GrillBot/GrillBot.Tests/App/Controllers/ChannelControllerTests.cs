using Discord.WebSocket;
using GrillBot.App.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class ChannelControllerTests
    {
        [TestMethod]
        public void GetChannelBoard_NoMutualGuilds()
        {
            var client = new DiscordSocketClient();
            var controller = new ChannelController(client, null, null);

            var result = controller.GetChannelboardAsync().Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }
    }
}
