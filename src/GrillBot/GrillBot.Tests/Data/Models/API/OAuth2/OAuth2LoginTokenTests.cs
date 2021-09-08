using GrillBot.Data.Models.API.OAuth2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.OAuth2
{
    [TestClass]
    public class OAuth2LoginTokenTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new OAuth2LoginToken());
        }

        [TestMethod]
        public void ErrorConstructor()
        {
            var token = new OAuth2LoginToken("error");
            Assert.AreEqual("error", token.ErrorMessage);
            Assert.IsNull(token.AccessToken);
        }

        [TestMethod]
        public void SuccessConstructor()
        {
            var token = new OAuth2LoginToken("Token", DateTimeOffset.MaxValue);

            Assert.AreEqual("Token", token.AccessToken);
            Assert.IsNull(token.ErrorMessage);
        }
    }
}
