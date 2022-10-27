using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V2.Events;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Guilds.GuildEvents;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V2.Events;

[TestClass]
public class CreateScheduledEventTests : ApiActionTest<CreateScheduledEvent>
{
    protected override CreateScheduledEvent CreateAction()
    {
        var texts = new TextsBuilder()
            .AddText("GuildScheduledEvents/GuildNotFound", "cs", "GuildNotFound")
            .Build();

        var guildEvent = new GuildScheduledEventBuilder().SetId(Consts.GuildEventId).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetCreateEventAction(guildEvent).Build();
        var discordClient = new ClientBuilder().SetGetGuildAction(guild).Build();

        return new CreateScheduledEvent(ApiRequestContext, discordClient, texts);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessAsync_GuildNotFound()
    {
        await Action.ProcessAsync(Consts.GuildId + 1, new ScheduledEventParams());
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutImage() => await ProcessAsync(null);

    [TestMethod]
    public async Task ProcessAsync_WithImage() => await ProcessAsync(new byte[] { 1, 2, 3, 4, 5 });

    private async Task ProcessAsync(byte[] image)
    {
        var parameters = new ScheduledEventParams
        {
            Description = "Description",
            Image = image,
            Location = "Location",
            Name = "Name",
            EndAt = DateTime.Now.AddDays(1),
            StartAt = DateTime.Now
        };

        var result = await Action.ProcessAsync(Consts.GuildId, parameters);
        Assert.AreEqual(Consts.GuildEventId, result);
    }
}
