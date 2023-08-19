using GrillBot.Core.Services.Graphics.Models.Chart;
using GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public static class ChartRequestBuilder
{
    public static ChartRequestData CreateCommonRequest()
    {
        return new ChartRequestData
        {
            Options = new ChartOptions
            {
                Height = 500,
                Width = 1920,
                BackgroundColor = "white",
                LegendPosition = "bottom",
                PointsRadius = 1
            },
            Data = new ChartData
            {
                TopLabel = new Label
                {
                    Color = "black",
                    Align = "center",
                    Size = 20,
                    Weight = "normal"
                }
            }
        };
    }

    public static IEnumerable<(DateOnly day, long points)> FilterData(IEnumerable<PointsChartItem> data, ChartsFilter filter)
    {
        var query = filter switch
        {
            ChartsFilter.Messages => data.Select(o => (o.Day, o.MessagePoints)),
            ChartsFilter.Reactions => data.Select(o => (o.Day, o.ReactionPoints)),
            _ => data.Select(o => (o.Day, o.MessagePoints + o.ReactionPoints))
        };

        return query.Where(o => o.Item2 > 0);
    }
}
