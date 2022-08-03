using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.User.Points;

public class PointsApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public PointsApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient, ApiRequestContext apiRequestContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        ApiRequestContext = apiRequestContext;
    }

    public async Task<PaginatedResponse<PointsTransaction>> GetTransactionListAsync(GetPointTransactionsParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var transactions = await repository.Points.GetTransactionListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<PointsTransaction>.CopyAndMapAsync(transactions, entity => Task.FromResult(Mapper.Map<PointsTransaction>(entity)));
    }

    public async Task<PaginatedResponse<PointsSummary>> GetSummariesAsync(GetPointsSummaryParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var summaries = await repository.Points.GetSummaryListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<PointsSummary>.CopyAndMapAsync(summaries, entity => Task.FromResult(Mapper.Map<PointsSummary>(entity)));
    }

    public async Task<List<PointsSummaryBase>> GetGraphDataAsync(GetPointsSummaryParams parameters)
    {
        parameters.Pagination.Page = 1;
        parameters.Pagination.PageSize = int.MaxValue;
        parameters.Sort.OrderBy = "Day";
        parameters.Sort.Descending = false;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetSummaryListAsync(parameters, parameters.Pagination);
        return data.Data.ConvertAll(entity => Mapper.Map<PointsSummaryBase>(entity));
    }

    public async Task<List<UserPointsItem>> GetPointsBoardAsync()
    {
        var result = new List<UserPointsItem>();
        var mutualGuilds = (await DiscordClient.FindMutualGuildsAsync(ApiRequestContext.LoggedUser!.Id)).ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetPointsBoardDataAsync(mutualGuilds);
        if (data.Count > 0)
            result.AddRange(Mapper.Map<List<UserPointsItem>>(data));

        return result;
    }
}
