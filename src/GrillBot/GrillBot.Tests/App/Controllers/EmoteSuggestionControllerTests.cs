using System;
using GrillBot.App.Controllers;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using EmoteSuggestion = GrillBot.Data.Models.API.Suggestions.EmoteSuggestion;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class EmoteSuggestionControllerTests : ControllerTest<EmoteSuggestionController>
{
    protected override bool CanInitProvider() => false;

    protected override EmoteSuggestionController CreateController()
    {
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new EmoteSuggestionApiService(DatabaseBuilder, mapper);

        return new EmoteSuggestionController(apiService);
    }

    [TestMethod]
    public async Task GetSuggestionListAsync_WithoutFilter()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var suggestion = new Database.Entity.EmoteSuggestion
        {
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = new byte[] { 1 },
            GuildId = guild.Id.ToString(),
            Guild = Guild.FromDiscord(guild),
            FromUserId = user.Id.ToString(),
            FromUser = GuildUser.FromDiscord(guild, user),
            Filename = "File",
            EmoteName = "emote"
        };

        await Repository.AddAsync(User.FromDiscord(user));
        await Repository.AddAsync(suggestion);
        await Repository.CommitAsync();

        var filter = new GetSuggestionsListParams();
        var result = await AdminController.GetSuggestionListAsync(filter);

        CheckResult<OkObjectResult, PaginatedResponse<EmoteSuggestion>>(result);
    }

    [TestMethod]
    public async Task GetSuggestionListAsync_WithFilter()
    {
        var filter = new GetSuggestionsListParams
        {
            CreatedAt = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
            EmoteName = "Emote",
            GuildId = Consts.GuildId.ToString(),
            FromUserId = Consts.UserId.ToString(),
            OnlyCommunityApproved = true,
            OnlyUnfinishedVotes = true,
            OnlyApprovedToVote = true,
            Sort = { Descending = true }
        };

        var result = await AdminController.GetSuggestionListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<EmoteSuggestion>>(result);
    }
}
