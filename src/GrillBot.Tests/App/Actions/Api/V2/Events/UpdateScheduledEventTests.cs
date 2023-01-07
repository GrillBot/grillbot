using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V2.Events;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Guilds.GuildEvents;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V2.Events;

[TestClass]
public class UpdateScheduledEventTests : ApiActionTest<UpdateScheduledEvent>
{
    protected override UpdateScheduledEvent CreateAction()
    {
        var events = new[]
        {
            new GuildScheduledEventBuilder(Consts.GuildEventId).SetCreator(ApiRequestContext.LoggedUser).Build(),
            new GuildScheduledEventBuilder(Consts.GuildEventId + 1).SetCreator(new UserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build()).Build()
        };

        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetEventsAction(events).Build();
        var discordClient = new ClientBuilder().SetGetGuildAction(guild).SetSelfUser(new SelfUserBuilder(ApiRequestContext.LoggedUser).Build()).Build();

        return new UpdateScheduledEvent(ApiRequestContext, discordClient, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound() => await Action.ProcessAsync(Consts.GuildId + 1, 0, new ScheduledEventParams());

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_EventNotFound() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId + 2, new ScheduledEventParams());

    [TestMethod]
    [ExpectedException(typeof(ForbiddenAccessException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Forbidden() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId + 1, new ScheduledEventParams());

    [TestMethod]
    public async Task ProcessAsync_WithoutParameters() => await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId, new ScheduledEventParams());

    [TestMethod]
    public async Task ProcessAsync_WithParameters()
    {
        await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId, new ScheduledEventParams
        {
            Description = "Description",
            Image = new byte[] { 1, 2, 3, 4, 5 },
            Location = "Location",
            Name = "Name",
            EndAt = DateTime.Now.AddDays(1),
            StartAt = DateTime.Now
        });
    }

    [TestMethod]
    public async Task ProcessAsync_SetEnded()
    {
        await Action.ProcessAsync(Consts.GuildId, Consts.GuildEventId, new ScheduledEventParams
        {
            StartAt = DateTime.Now.AddMonths(-5),
            EndAt = DateTime.Now.AddMonths(-1),
        });
    }
}
