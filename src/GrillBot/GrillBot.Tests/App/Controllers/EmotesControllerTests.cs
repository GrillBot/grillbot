using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class EmotesControllerTests : ControllerTest<EmotesController>
{
    protected override bool CanInitProvider() => false;

    protected override EmotesController CreateController(IServiceProvider provider)
    {
        var discordClient = DiscordHelper.CreateClient();
        var cacheService = new EmotesCacheService(discordClient);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new EmotesApiService(DbFactory, cacheService, mapper);

        return new EmotesController(apiService);
    }

    [TestMethod]
    public async Task GetStatsOfSupportedEmotesAsync_WithoutFilter()
    {
        var @params = new EmotesListParams();
        var result = await AdminController.GetStatsOfSupportedEmotesAsync(@params, CancellationToken.None);

        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task GetStatsOfSupportedEmotesAsync_WithFilter()
    {
        var @params = new EmotesListParams()
        {
            FirstOccurence = new RangeParams<DateTime?>() { From = DateTime.MinValue, To = DateTime.MaxValue },
            GuildId = Consts.GuildId.ToString(),
            LastOccurence = new RangeParams<DateTime?>() { From = DateTime.MinValue, To = DateTime.MaxValue },
            Sort = new SortParams() { Descending = true, OrderBy = "EmoteId" },
            UseCount = new RangeParams<int?>() { From = 0, To = 50 },
            UserId = Consts.UserId.ToString(),
            Pagination = new PaginatedParams(),
            FilterAnimated = true
        };

        var result = await AdminController.GetStatsOfSupportedEmotesAsync(@params, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task GetStatsOfUnsupportedEmotesAsync()
    {
        var @params = new EmotesListParams();
        @params.Sort.OrderBy = "FirstOccurence";

        var result = await AdminController.GetStatsOfUnsupportedEmotesAsync(@params, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<EmoteStatItem>>(result);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync()
    {
        var @params = new MergeEmoteStatsParams()
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

        await DbContext.AddAsync(new EmoteStatisticItem()
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
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }
}
