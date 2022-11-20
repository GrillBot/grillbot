using Discord;
using GrillBot.App.Actions.Commands.Reminder;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Reminder;

[TestClass]
public class GetReminderListTests : CommandActionTest<GetReminderList>
{
    private static readonly IGuildUser UserData = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
    private static readonly IDiscordClient ClientData = new ClientBuilder().SetGetUserAction(UserData).Build();

    protected override IGuildUser User => UserData;
    protected override IDiscordClient Client => ClientData;

    protected override GetReminderList CreateAction()
    {
        var apiContext = new ApiRequestContext();
        var apiAction = new GrillBot.App.Actions.Api.V1.Reminder.GetReminderList(apiContext, DatabaseBuilder, TestServices.AutoMapper.Value);
        var texts = new TextsBuilder()
            .AddText("RemindModule/List/Embed/Title", "en-US", "{0}")
            .AddText("RemindModule/List/Embed/NoItems", "en-US", "{0}")
            .AddText("RemindModule/List/Embed/RowTitle", "en-US", "{0},{1},{2},{3}")
            .Build();

        return InitAction(new GetReminderList(apiAction, texts, DatabaseBuilder));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(User),
            FromUserId = User.Id.ToString(),
            Id = 1,
            Message = "Message",
            OriginalMessageId = "12345",
            ToUser = Database.Entity.User.FromDiscord(User),
            ToUserId = User.Id.ToString(),
            Language = "cs"
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ComputePagesCountAsync()
    {
        var result = await Action.ComputePagesCountAsync();
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutData()
    {
        var (embed, paginationComponents) = await Action.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNotNull(embed.Footer);
        Assert.AreEqual(0, embed.Fields.Length);
        Assert.IsFalse(string.IsNullOrEmpty(embed.Description));
        Assert.IsNull(paginationComponents);
    }

    [TestMethod]
    public async Task ProcessAsync_WithData()
    {
        await InitDataAsync();

        var (embed, paginationComponents) = await Action.ProcessAsync(0);

        Assert.IsNotNull(embed);
        Assert.IsNotNull(embed.Footer);
        Assert.AreNotEqual(0, embed.Fields.Length);
        Assert.IsTrue(string.IsNullOrEmpty(embed.Description));
        Assert.IsNull(paginationComponents);
    }
}
