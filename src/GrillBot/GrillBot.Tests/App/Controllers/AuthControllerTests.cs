using GrillBot.Data.Controllers;
using GrillBot.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        [TestMethod]
        public void GetRedirectLink()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "OAuth2:ClientId", "12345" },
                    { "OAuth2:RedirectUrl", "http://localhost" }
                }).Build();

            var service = new OAuth2Service(configuration, null, null);
            var controller = new AuthController(service);

            var result = controller.GetRedirectLink(false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }
    }
}
