using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Logging;
using GrillBot.Tests.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure
{
    [TestClass]
    public class ErrorHandlingMiddlewareTests
    {
        [TestMethod]
        public void Invoke_OK()
        {
            RequestDelegate @delegate = (HttpContext _) => Task.CompletedTask;
            var middleware = new ErrorHandlingMiddleware(@delegate, null);

            middleware.InvokeAsync(null).Wait();
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Invoke_Error()
        {
            static Task @delegate(HttpContext _) => Task.FromException(new Exception("Test"));

            var client = new DiscordSocketClient();
            var commandService = new CommandService();
            var configuration = ConfigHelpers.CreateConfiguration();
            var interactions = new InteractionService(client);
            var logging = new LoggingService(client, commandService, null, configuration, null, interactions);
            var middleware = new ErrorHandlingMiddleware(@delegate, logging);

            var request = new Mock<HttpRequest>();
            request.Setup(o => o.Path).Returns("/api");
            var context = new Mock<HttpContext>();
            context.Setup(o => o.Request).Returns(request.Object);
            middleware.InvokeAsync(context.Object).Wait();
        }
    }
}
