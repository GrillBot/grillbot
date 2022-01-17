using GrillBot.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class OAuth2ServiceTests
    {
        [TestMethod]
        public void GetRedirectLink_Private()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "OAuth2:ClientId", "12345" },
                    { "OAuth2:RedirectUrl", "http://localhost" }
                }).Build();

            var service = new OAuth2Service(configuration, null, null);
            var result = service.GetRedirectLink(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(
                "https://discord.com:443/api/oauth2/authorize?client_id=12345&redirect_uri=http%3A%2F%2Flocalhost&response_type=code&scope=identify&state=False",
                result.Url
            );
        }

        [TestMethod]
        public void GetRedirectLink_Public()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "OAuth2:ClientId", "12345" },
                    { "OAuth2:RedirectUrl", "http://localhost" }
                }).Build();

            var service = new OAuth2Service(configuration, null, null);
            var result = service.GetRedirectLink(true);

            Assert.IsNotNull(result);
            Assert.AreEqual(
                "https://discord.com:443/api/oauth2/authorize?client_id=12345&redirect_uri=http%3A%2F%2Flocalhost&response_type=code&scope=identify&state=True",
                result.Url
            );
        }
    }
}
