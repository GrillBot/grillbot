using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Modules.Implementations.Reminder;

[TestClass]
public class RemindPostponeHandlerTests : TestBase<RemindPostponeHandler>
{
    protected override RemindPostponeHandler CreateInstance()
    {
        return new RemindPostponeHandler(1, TestServices.Provider.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_NotDms()
    {
        var interaction = new DiscordInteractionBuilder(Consts.InteractionId).AsDmInteraction(false).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await Instance.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync_NoComponentInteraction()
    {
        var interaction = new DiscordInteractionBuilder(Consts.InteractionId).AsDmInteraction().Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await Instance.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync_UnknownRemind()
    {
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        var interaction = new ComponentInteractionBuilder(Consts.InteractionId).AsDmInteraction().SetMessage(message).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await Instance.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var message = new UserMessageBuilder(Consts.MessageId).Build();
        var interaction = new ComponentInteractionBuilder(Consts.InteractionId).AsDmInteraction().SetMessage(message).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build()),
            Id = 42,
            Message = "ASDF",
            OriginalMessageId = "425639",
            Postpone = 0,
            RemindMessageId = Consts.MessageId.ToString(),
            ToUser = Database.Entity.User.FromDiscord(new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build()),
            Language = "cs"
        });
        await Repository.CommitAsync();

        await Instance.ProcessAsync(context);
    }
}
