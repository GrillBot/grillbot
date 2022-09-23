using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using GrillBot.App.Services.AuditLog;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class EmotesControllerTests : ControllerTest<EmotesController>
{
    protected override EmotesController CreateController()
    {
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new EmotesApiService(DatabaseBuilder, ApiRequestContext, auditLogWriter);

        return new EmotesController(apiService, ServiceProvider);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_NoEmotes()
    {
        var result = await Controller.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_WithEmotes()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = DateTime.MinValue,
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = GuildUser.FromDiscord(guild, guildUser),
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var result = await Controller.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }
}
