using GrillBot.App.Services.User.Points;
using GrillBot.Cache.Services.Managers;

namespace GrillBot.Tests.App.Services.User.Points;

[TestClass]
public class PointsJobTests : JobTest<PointsJob>
{
    protected override PointsJob CreateJob()
    {
        var configuration = TestServices.Configuration.Value;
        var counter = TestServices.CounterManager.Value;
        var profilePictures = new ProfilePictureManager(CacheBuilder, counter);
        var texts = new TextsBuilder().Build();
        var pointsService = new PointsService(DatabaseBuilder, configuration, TestServices.Randomization.Value, profilePictures, texts);

        return new PointsJob(pointsService, TestServices.InitializedProvider.Value);
    }

    [TestMethod]
    public async Task RunAsync()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNotNull(context.Result);
    }
}
