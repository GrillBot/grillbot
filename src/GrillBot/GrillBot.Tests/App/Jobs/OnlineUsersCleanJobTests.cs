using Discord;
using GrillBot.App.Jobs;
using GrillBot.Database.Enums;

namespace GrillBot.Tests.App.Jobs;

[TestClass]
public class OnlineUsersCleanJobTests : JobTest<OnlineUsersCleanJob>
{
    protected override OnlineUsersCleanJob CreateJob()
    {
        return new OnlineUsersCleanJob(DatabaseBuilder, TestServices.InitializedProvider.Value);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.User
            {
                Id = Consts.UserId.ToString(),
                Discriminator = Consts.Discriminator,
                Flags = (int)UserFlags.WebAdminOnline,
                Status = UserStatus.Online,
                Username = Consts.Username
            },
            new Database.Entity.User
            {
                Id = (Consts.UserId + 1).ToString(),
                Discriminator = Consts.Discriminator,
                Flags = (int)UserFlags.PublicAdminOnline,
                Status = UserStatus.Online,
                Username = Consts.Username
            }
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task Execute_NoUsers()
    {
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task Execute_Ok()
    {
        await InitDataAsync();
        var context = CreateContext();
        await Job.Execute(context);

        Assert.IsNotNull(context.Result);
        Assert.IsInstanceOfType(context.Result, typeof(string));
        Assert.IsTrue(((string)context.Result).Contains("LoggedUsers"));
    }
}
