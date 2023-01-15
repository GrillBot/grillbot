using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V2.Events;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V2.Events;

[TestClass]
public class CancelScheduledEventTests : ApiActionTest<CancelScheduledEvent>
{
    protected override CancelScheduledEvent CreateAction()
    {
        var events = new[]
        {
            new GuildScheduledEventBuilder(Consts.GuildEventId).SetCreator(ApiRequestContext.LoggedUser).SetStatus(GuildScheduledEventStatus.Scheduled).SetEndDate(DateTimeOffset.MaxValue)
                .Build(), // Success
            new GuildScheduledEventBuilder(Consts.GuildEventId + 1).SetCreator(new UserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build()).Build(), // Forbidden
            new GuildScheduledEventBuilder(Consts.GuildEventId + 2).SetCreator(ApiRequestContext.LoggedUser).SetStatus(GuildScheduledEventStatus.Completed).Build()
        };

        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetEventsAction(events).Build();
        var discordClient = new ClientBuilder().SetGetGuildsAction(new[] { guild }).SetSelfUser(new SelfUserBuilder(ApiRequestContext.LoggedUser).Build()).Build();

        return new CancelScheduledEvent(ApiRequestContext, discordClient, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() => await Action.ProcessAsync(Consts.GuildId + 1, 0);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_EventNotFound() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId + 5);

    [TestMethod]
    [ExpectedException(typeof(ForbiddenAccessException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Forbidden() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId + 1);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_CannotCancel() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId + 2);

    [TestMethod]
    public async Task ProcessAsync_Success() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId);
}
