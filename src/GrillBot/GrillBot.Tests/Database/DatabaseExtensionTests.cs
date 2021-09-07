using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GrillBot.Database;
using GrillBot.Database.Services;

namespace GrillBot.Tests.Database
{
    [TestClass]
    public class DatabaseExtensionTests
    {
        [TestMethod]
        public void AddDatabase()
        {
            var collection = new ServiceCollection()
                .AddDatabase("Host=test;Database=GrillBot;Username=postgres;Password=test");

            using var provider = collection.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetService<GrillBotContext>();

            Assert.IsNotNull(context);
        }
    }
}
