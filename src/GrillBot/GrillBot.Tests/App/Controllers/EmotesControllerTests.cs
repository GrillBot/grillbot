using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;
using GrillBot.App.Services.AuditLog;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class EmotesControllerTests : ControllerTest<EmotesController>
{
    protected override bool CanInitProvider() => false;

    protected override EmotesController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var cacheService = new EmotesCacheService(discordClient);
        var mapper = AutoMapperHelper.CreateMapper();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new EmotesApiService(DatabaseBuilder, cacheService, mapper, AdminApiRequestContext, auditLogWriter);

        return new EmotesController(apiService);
    }

    [TestMethod]
    public async Task GetStatsOfSupportedEmotesAsync_WithoutFilter()
    {
        var @params = new EmotesListParams();
        var result = await AdminController.GetStatsOfSupportedEmotesAsync(@params);

        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task GetStatsOfSupportedEmotesAsync_WithFilter()
    {
        var @params = new EmotesListParams
        {
            FirstOccurence = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Consts.GuildId.ToString(),
            LastOccurence = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            Sort = new SortParams { Descending = true, OrderBy = "EmoteId" },
            UseCount = new RangeParams<int?> { From = 0, To = 50 },
            UserId = Consts.UserId.ToString(),
            Pagination = new PaginatedParams(),
            FilterAnimated = true,
            EmoteName = "Emote"
        };

        var result = await AdminController.GetStatsOfSupportedEmotesAsync(@params);
        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task GetStatsOfUnsupportedEmotesAsync()
    {
        var @params = new EmotesListParams
        {
            Sort = { OrderBy = "FirstOccurence" }
        };

        var result = await AdminController.GetStatsOfUnsupportedEmotesAsync(@params);
        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync()
    {
        var @params = new MergeEmoteStatsParams
        {
            SourceEmoteId = Consts.FeelsHighManEmote,
            DestinationEmoteId = Consts.PepeJamEmote
        };

        var result = await AdminController.MergeStatsToAnotherAsync(@params);
        CheckResult<BadRequestObjectResult, int>(result);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_NoEmotes()
    {
        var result = await AdminController.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_WithEmotes()
    {
        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .Build();

        var guildUser = new GuildUserBuilder()
            .SetId(Consts.UserId).SetUsername(Consts.Username)
            .SetGuild(guild).SetDiscriminator(Consts.Discriminator)
            .Build();

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

        var result = await AdminController.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }
}
