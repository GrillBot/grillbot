using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        var guildEvent = new GuildScheduledEventBuilder(Consts.GuildEventId).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetCreateEventAction(guildEvent).Build();
        var discordClient = new ClientBuilder().SetGetGuildAction(guild).Build();

        return new CreateScheduledEvent(ApiRequestContext, discordClient, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessAsync_GuildNotFound() => await ProcessAsync(null, Consts.GuildId + 1);

    [TestMethod]
    public async Task ProcessAsync_WithoutImage() => await ProcessAsync(null, Consts.GuildId);

    [TestMethod]
    public async Task ProcessAsync_WithImage() => await ProcessAsync(new byte[] { 1, 2, 3, 4, 5 }, Consts.GuildId);

    [TestMethod]
    public async Task ProcessAsync_ValidationFailed()
    {
        var cases = new[]
        {
            new ScheduledEventParams(),
            new ScheduledEventParams { Name = "Name" },
            new ScheduledEventParams { Name = "Name", StartAt = DateTime.Now },
            new ScheduledEventParams { Name = "Name", StartAt = DateTime.Now, EndAt = DateTime.Now },
            new ScheduledEventParams { Name = "Name", StartAt = DateTime.Now, EndAt = DateTime.Now, Location = "Location" }
        };

        foreach (var @case in cases)
        {
            try
            {
                await ProcessAsync(null, Consts.GuildId, @case);
            }
            catch (Exception ex)
            {
                var vex = ex as ValidationException;
                if (vex == null)
                    Assert.Fail("Exception type is not ValidationException");

                Assert.IsNotNull(vex.ValidationResult);
                Assert.IsTrue(vex.Value == null || (vex.Value is DateTime dt && dt == DateTime.MinValue));
                Assert.IsFalse(string.IsNullOrEmpty(vex.ValidationResult.ErrorMessage));
                Assert.IsTrue(vex.ValidationResult.MemberNames.Any());
            }
        }
    }

    private async Task ProcessAsync(byte[] image, ulong guildId, ScheduledEventParams parameters = null)
    {
        parameters ??= new ScheduledEventParams
        {
            Description = "Description",
            Location = "Location",
            Name = "Name",
            EndAt = DateTime.Now.AddDays(1),
            StartAt = DateTime.Now
        };
        parameters.Image = image;

        var result = await Action.ProcessAsync(guildId, parameters);
        Assert.AreEqual(Consts.GuildEventId, result);
    }
}
