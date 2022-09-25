using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetSummaries : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetSummaries(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<PointsSummary>> ProcessAsync(GetPointsSummaryParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var summaries = await repository.Points.GetSummaryListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<PointsSummary>.CopyAndMapAsync(summaries, entity =>
        {
            var item = Mapper.Map<PointsSummary>(entity);
            if (entity.IsMerged)
                item.MergeInfo = Mapper.Map<PointsMergeInfo>(entity);
            return Task.FromResult(item);
        });
    }
}
