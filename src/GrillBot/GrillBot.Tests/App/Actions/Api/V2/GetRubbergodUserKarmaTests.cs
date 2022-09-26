using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V2;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json.Linq;

namespace GrillBot.Tests.App.Actions.Api.V2;

[TestClass]
public class GetRubbergodUserKarmaTests : ApiActionTest<GetRubbergodUserKarma>
{
    private readonly JObject _successData = new()
    {
        { "meta", JObject.Parse("{\"page\": 1, \"items_count\": 4103, \"total_pages\": 83}") },
        {
            "content",
            JArray.Parse("[{\"karma\": 147407, \"positive\": 19541, \"member_ID\": \"624160228111417344\", \"negative\": 4729}, {\"karma\": 147407, \"positive\": 19541, \"member_ID\": \"" +
                         Consts.UserId + "\", \"negative\": 4729}]")
        }
    };

    protected override GetRubbergodUserKarma CreateAction()
    {
        var directApi = new DirectApiBuilder()
            .SetSendCommandAction("Rubbergod", "Karma|asc|karma|1", "{}")
            .SetSendCommandAction("Rubbergod", "Karma|asc|karma|2", "{\"meta\": {}}")
            .SetSendCommandAction("Rubbergod", "Karma|asc|karma|3", _successData.ToString())
            .Build();
        var client = new ClientBuilder()
            .SetGetGuildsAction(Enumerable.Empty<IGuild>())
            .SetGetUserAction(new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build())
            .Build();

        return new GetRubbergodUserKarma(ApiRequestContext, directApi, client, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_MissingMetadata()
    {
        // Karma|desc|karma|0
        var parameters = new KarmaListParams { Pagination = { Page = 0 } };
        await Action.ProcessAsync(parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(GrillBotException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_MissingContent()
    {
        // Karma|desc|karma|2
        var parameters = new KarmaListParams { Pagination = { Page = 1 } };
        await Action.ProcessAsync(parameters);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var parameters = new KarmaListParams { Pagination = { Page = 2 } };
        var result = await Action.ProcessAsync(parameters);

        Assert.IsNotNull(result?.Data);
        Assert.AreEqual(1, result.Data.Count);
    }
}
