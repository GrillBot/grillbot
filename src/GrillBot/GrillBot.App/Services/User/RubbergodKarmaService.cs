using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.DirectApi;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Services.User;

public class RubbergodKarmaService : ServiceBase
{
    private DirectApiService DirectApi { get; }

    public RubbergodKarmaService(DirectApiService directApi, IDiscordClient client, IMapper mapper) : base(null, null, client, mapper)
    {
        DirectApi = directApi;
    }

    public async Task<PaginatedResponse<UserKarma>> GetUserKarmaAsync(SortParams sort, PaginatedParams pagination, CancellationToken cancellationToken = default)
    {
        var command = CommandBuilder.CreateKarmaCommand(sort, pagination);
        var jsonData = await DirectApi.SendCommandAsync("Rubbergod", command, cancellationToken);
        var data = JObject.Parse(jsonData);

        var result = new PaginatedResponse<UserKarma>()
        {
            Data = new List<UserKarma>(),
            Page = data["meta"]["page"].Value<int>(),
            TotalItemsCount = data["meta"]["items_count"].Value<int>()
        };

        result.CanPrev = result.Page > 1;
        result.CanNext = result.Page < data["meta"]["total_pages"].Value<int>();

        var itemsOnPage = data["content"].OfType<JObject>().ToList();
        for (int i = 0; i < itemsOnPage.Count; i++)
        {
            var position = pagination.Skip + i + 1;
            var row = await ParseRowAsync(itemsOnPage[i], position);
            if (row != null)
                result.Data.Add(row);
        }

        return result;
    }

    private async Task<UserKarma> ParseRowAsync(JObject row, int position)
    {
        var memberId = row["member_ID"].Value<ulong>();
        var user = await DcClient.FindUserAsync(memberId);
        if (user == null)
            return null;

        return new UserKarma()
        {
            Negative = row["negative"].Value<int>(),
            Value = row["karma"].Value<int>(),
            Positive = row["positive"].Value<int>(),
            User = Mapper.Map<Data.Models.API.Users.User>(user),
            Position = position
        };
    }
}
