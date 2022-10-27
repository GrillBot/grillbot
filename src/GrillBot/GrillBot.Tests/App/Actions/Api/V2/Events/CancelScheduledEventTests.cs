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
        var texts = new TextsBuilder()
            .AddText("GuildScheduledEvents/GuildNotFound", "cs", "GuildNotFound")
            .AddText("GuildScheduledEvents/EventNotFound", "cs", "EventNotFound")
            .AddText("GuildScheduledEvents/ForbiddenAccess", "cs", "Forbidden")
            .Build();

        var events = new[]
        {
            new GuildScheduledEventBuilder().SetId(Consts.GuildEventId).SetCreator(ApiRequestContext.LoggedUser).SetStatus(GuildScheduledEventStatus.Scheduled).SetEndDate(DateTimeOffset.MaxValue)
                .Build(), // Success
            new GuildScheduledEventBuilder().SetId(Consts.GuildEventId + 1)
                .SetCreator(new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build()).Build(), // Forbidden
            new GuildScheduledEventBuilder().SetId(Consts.GuildEventId + 2).SetCreator(ApiRequestContext.LoggedUser).SetStatus(GuildScheduledEventStatus.Completed).Build()
        };

        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetEventsAction(events).Build();
        var discordClient = new ClientBuilder().SetGetGuildAction(guild).SetSelfUser(new SelfUserBuilder(ApiRequestContext.LoggedUser).Build()).Build();

        return new CancelScheduledEvent(ApiRequestContext, discordClient, texts);
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
