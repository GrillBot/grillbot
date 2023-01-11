using GrillBot.App.Services.Graphics;
using GrillBot.App.Services.Graphics.Models.Chart;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public class GuildChartRenderer
{
    private ITextsManager Texts { get; }
    private IGraphicsClient GraphicsClient { get; }

    public GuildChartRenderer(ITextsManager texts, IGraphicsClient graphicsClient)
    {
        Texts = texts;
        GraphicsClient = graphicsClient;
    }

    public async Task<MagickImage> RenderAsync(IGuild guild, IEnumerable<(DateTime day, int messagePoints, int reactionPoints)> data, ChartsFilter filter,
        string locale)
    {
        var request = CreateRequest(filter, data, guild, locale);
        var graphData = await GraphicsClient.CreateChartAsync(request);

        return new MagickImage(graphData);
    }

    private ChartRequestData CreateRequest(ChartsFilter filter, IEnumerable<(DateTime day, int messagePoints, int reactionPoints)> data, IGuild guild,
        string locale)
    {
        var request = ChartRequestBuilder.CreateCommonRequest();

        request.Data.TopLabel!.Text = filter switch
        {
            ChartsFilter.Messages => Texts["Points/Chart/Title/Guild/Messages", locale],
            ChartsFilter.Reactions => Texts["Points/Chart/Title/Guild/Reactions", locale],
            _ => Texts["Points/Chart/Title/Guild/Summary", locale]
        };

        var filteredData = ChartRequestBuilder.FilterData(data, filter).ToList();

        request.Data.Datasets.Add(new Dataset
        {
            Data = filteredData.ConvertAll(o => new DataPoint
            {
                Label = o.day.ToCzechFormat(true),
                Value = filteredData.Where(x => x.day <= o.day).Sum(x => x.points)
            }),
            Color = "black",
            Label = guild.Name,
            Width = 1
        });

        return request;
    }
}
