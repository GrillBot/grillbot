using GrillBot.App.Modules.Implementations.Reminder;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Modules.Implementations.Reminder;

[TestClass]
public class RemindPostponeHandlerTests : ServiceTest<RemindPostponeHandler>
{
    protected override RemindPostponeHandler CreateService() => null;

    [TestMethod]
    public async Task ProcessAsync_NotDms()
    {
        var handler = new RemindPostponeHandler(1, TestServices.EmptyProvider.Value);
        var interaction = new DiscordInteractionBuilder().SetDmInteraction(false).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await handler.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync_NoComponentInteraction()
    {
        var handler = new RemindPostponeHandler(1, TestServices.EmptyProvider.Value);
        var interaction = new DiscordInteractionBuilder().SetDmInteraction().Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await handler.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync_UnknownRemind()
    {
        var handler = new RemindPostponeHandler(1, TestServices.InitializedProvider.Value);
        var message = new UserMessageBuilder().SetId(Consts.MessageId).Build();
        var interaction = new ComponentInteractionBuilder().SetDmInteraction().SetMessage(message).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await handler.ProcessAsync(context);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var handler = new RemindPostponeHandler(1, TestServices.InitializedProvider.Value);
        var message = new UserMessageBuilder().SetId(Consts.MessageId).Build();
        var interaction = new ComponentInteractionBuilder().SetDmInteraction().SetMessage(message).Build();
        var context = new InteractionContextBuilder().SetInteraction(interaction).Build();

        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUser = Database.Entity.User.FromDiscord(new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build()),
            Id = 42,
            Message = "ASDF",
            OriginalMessageId = "425639",
            Postpone = 0,
            RemindMessageId = Consts.MessageId.ToString(),
            ToUser = Database.Entity.User.FromDiscord(new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build()),
            Language = "cs"
        });
        await Repository.CommitAsync();

        await handler.ProcessAsync(context);
    }
}
