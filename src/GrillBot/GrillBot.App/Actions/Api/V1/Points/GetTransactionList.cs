using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetTransactionList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetTransactionList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<PointsTransaction>> ProcessAsync(GetPointTransactionsParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var transactions = await repository.Points.GetTransactionListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<PointsTransaction>.CopyAndMapAsync(transactions, entity =>
        {
            var item = Mapper.Map<PointsTransaction>(entity);
            if (entity.MergedItemsCount > 0)
                item.MergeInfo = Mapper.Map<PointsMergeInfo>(entity);
            return Task.FromResult(item);
        });
    }
}
