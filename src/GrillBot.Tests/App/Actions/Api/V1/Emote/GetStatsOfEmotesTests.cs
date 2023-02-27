using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class GetStatsOfEmotesTests : ApiActionTest<GetStatsOfEmotes>
{
    protected override GetStatsOfEmotes CreateInstance()
    {
        var parsedEmote = Discord.Emote.Parse(Consts.OnlineEmoteId);
        var emote = EmoteHelper.CreateGuildEmote(parsedEmote);
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { emote }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { guild }).Build();

        var emoteHelper = new GrillBot.App.Helpers.EmoteHelper(client);
        return new GetStatsOfEmotes(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, emoteHelper);
    }

    [TestMethod]
    public async Task ProcessAsync_Supported_WithoutFilter()
    {
        var filter = new EmotesListParams();
        var result = await Instance.ProcessAsync(filter, false);

        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_Supported_WithFilter()
    {
        var sorts = new[] { "UseCount", "FirstOccurence", "LastOccurence", "EmoteId" };

        foreach (var sort in sorts)
        {
            foreach (var sortType in new[] { true, false })
            {
                var filter = new EmotesListParams
                {
                    FirstOccurence = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
                    GuildId = Consts.GuildId.ToString(),
                    LastOccurence = new RangeParams<DateTime?> { From = DateTime.MinValue, To = DateTime.MaxValue },
                    Sort = new SortParams { Descending = sortType, OrderBy = sort },
                    UseCount = new RangeParams<int?> { From = 0, To = 50 },
                    UserId = Consts.UserId.ToString(),
                    Pagination = new PaginatedParams(),
                    FilterAnimated = true,
                    EmoteName = "Emote"
                };

                var result = await Instance.ProcessAsync(filter, false);
                Assert.AreEqual(0, result.TotalItemsCount);
            }
        }
    }

    [TestMethod]
    public async Task ProcessAsync_Unsupported()
    {
        var filter = new EmotesListParams();
        var result = await Instance.ProcessAsync(filter, true);

        Assert.AreEqual(0, result.TotalItemsCount);
    }
}
