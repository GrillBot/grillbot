using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetSummaryGraphData : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetSummaryGraphData(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<List<PointsSummaryBase>> ProcessAsync(GetPointsSummaryParams parameters)
    {
        parameters.Sort.OrderBy = "Day";
        parameters.Sort.Descending = false;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetGraphDataAsync(parameters);
        return data.ConvertAll(o => Mapper.Map<PointsSummaryBase>(o));
    }
}
