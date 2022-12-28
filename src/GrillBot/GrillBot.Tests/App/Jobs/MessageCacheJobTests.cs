using GrillBot.App.Jobs;
using GrillBot.Common.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class MessageCacheJobTests : JobTest<MessageCacheJob>
{
    protected override MessageCacheJob CreateJob()
    {
        var provider = TestServices.InitializedProvider.Value;
        provider.GetRequiredService<InitManager>().Set(true);

        var messageCache = new MessageCacheBuilder().SetProcessScheduledTaskAction("Test").Build();
        return new MessageCacheJob(provider, messageCache);
    }

    [TestMethod]
    public async Task Execute()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNotNull(context.Result);
        Assert.AreEqual("Test", context.Result);
    }
}
