using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.Graphics.Models.Chart;
using GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class GuildChartBuilder : ChartBuilderBase<IEnumerable<PointsChartItem>>
{
    public GuildChartBuilder(ITextsManager texts) : base(texts)
    {
    }

    protected override string CreateTopLabel(ChartsFilter filter, string locale)
    {
        return filter switch
        {
            ChartsFilter.Messages => Texts["Points/Chart/Title/Guild/Messages", locale],
            ChartsFilter.Reactions => Texts["Points/Chart/Title/Guild/Reactions", locale],
            _ => Texts["Points/Chart/Title/Guild/Summary", locale]
        };
    }

    protected override IAsyncEnumerable<Dataset> CreateDatasetsAsync(ChartsFilter filter)
    {
        var filteredData = ChartRequestBuilder.FilterData(Data, filter)
            .OrderBy(o => o.day).ToList();

        return new List<Dataset>
        {
            new()
            {
                Color = "black",
                Label = Guild.Name,
                Width = 1,
                Data = filteredData.Select(o => new DataPoint
                {
                    Label = o.day.ToCzechFormat(),
                    Value = Convert.ToInt32(filteredData.Where(x => x.day <= o.day).Sum(x => x.points))
                }).ToList()
            }
        }.ToAsyncEnumerable();
    }
}
