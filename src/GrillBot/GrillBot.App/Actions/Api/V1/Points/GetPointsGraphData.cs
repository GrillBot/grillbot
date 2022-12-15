using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetPointsGraphData : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetPointsGraphData(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<PointsSummaryBase>> ProcessAsync(GetPointTransactionsParams parameters)
    {
        parameters.Sort.OrderBy = "Day";
        parameters.Sort.Descending = false;

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Points.GetGraphDataAsync(parameters);
        return data.ConvertAll(o => new PointsSummaryBase
        {
            Day = o.day,
            MessagePoints = o.messagePoints,
            ReactionPoints = o.reactionPoints,
            TotalPoints = o.messagePoints + o.reactionPoints
        });
    }
}
