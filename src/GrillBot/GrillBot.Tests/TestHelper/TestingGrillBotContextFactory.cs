using GrillBot.Database.Services;
using System;

namespace GrillBot.Tests.TestHelper
{
    public class TestingGrillBotContextFactory : GrillBotContextFactory
    {
        public TestingGrillBotContextFactory(IServiceProvider provider) : base(provider)
        {
        }

        public override GrillBotContext Create()
        {
            return ServiceProvider.GetService(typeof(TestingGrillBotContext)) as GrillBotContext;
        }
    }
}
