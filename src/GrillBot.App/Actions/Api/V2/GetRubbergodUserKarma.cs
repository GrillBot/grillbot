using AutoMapper;
using GrillBot.App.Services.DirectApi;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V2;

public class GetRubbergodUserKarma : ApiAction
{
    private IDirectApiService DirectApi { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }

    public GetRubbergodUserKarma(ApiRequestContext apiContext, IDirectApiService directApi, IDiscordClient discordClient, IMapper mapper) : base(apiContext)
    {
        DirectApi = directApi;
        DiscordClient = discordClient;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<UserKarma>> ProcessAsync(KarmaListParams parameters)
    {
        var command = CommandBuilder.CreateKarmaCommand(parameters.Sort, parameters.Pagination);
        var jsonData = await DirectApi.SendCommandAsync("Rubbergod", command);
        var data = JObject.Parse(jsonData);

        if (data["meta"] == null)
            throw new GrillBotException("Missing required field \"meta\" in rubbergod API output.");
        if (data["content"] == null)
            throw new GrillBotException("Missing required field \"content\" in rubbergod API output.");

        var result = new PaginatedResponse<UserKarma>
        {
            Data = new List<UserKarma>(),
            Page = data["meta"]["page"]!.Value<int>(),
            TotalItemsCount = data["meta"]["items_count"]!.Value<int>()
        };

        result.CanPrev = result.Page > 1;
        result.CanNext = result.Page < data["meta"]["total_pages"]!.Value<int>();

        var itemsOnPage = data["content"].OfType<JObject>().ToList();
        for (var i = 0; i < itemsOnPage.Count; i++)
        {
            var position = parameters.Pagination.Skip + i + 1;
            var row = await ParseRowAsync(itemsOnPage[i], position);
            if (row != null)
                result.Data.Add(row);
        }

        return result;
    }

    private async Task<UserKarma> ParseRowAsync(JObject row, int position)
    {
        var memberId = row["member_ID"].Value<ulong>();
        var user = await DiscordClient.FindUserAsync(memberId);
        if (user == null)
            return null;

        return new UserKarma
        {
            Negative = row["negative"].Value<int>(),
            Value = row["karma"].Value<int>(),
            Positive = row["positive"].Value<int>(),
            User = Mapper.Map<User>(user),
            Position = position
        };
    }
}
