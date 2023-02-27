using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class GetEmoteSuggestionListTests : ApiActionTest<GetEmoteSuggestionsList>
{
    protected override GetEmoteSuggestionsList CreateInstance()
    {
        return new GetEmoteSuggestionsList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_NoFilter()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var suggestion = new Database.Entity.EmoteSuggestion
        {
            SuggestionMessageId = Consts.MessageId.ToString(),
            CreatedAt = DateTime.Now,
            ImageData = new byte[] { 1 },
            GuildId = guild.Id.ToString(),
            Guild = Database.Entity.Guild.FromDiscord(guild),
            FromUserId = user.Id.ToString(),
            FromUser = Database.Entity.GuildUser.FromDiscord(guild, user),
            Filename = "File",
            EmoteName = "emote"
        };

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(suggestion);
        await Repository.CommitAsync();

        var filter = new GetSuggestionsListParams();
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
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

        var result = await Instance.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }
}
