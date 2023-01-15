using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using ImageMagick;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class GraphicsClientBuilder : BuilderBase<IGraphicsClient>
{
    public GraphicsClientBuilder SetAll()
        => SetCreateChartAction().SetVersionAction().SetGetMetricsAction().SetGetStatisticsAction();
    
    public GraphicsClientBuilder SetCreateChartAction()
    {
        using var image = new MagickImage(MagickColors.White, 100, 100);
        new Drawables().Line(0, 50, 50, 0).Draw(image);
        var bytes = image.ToByteArray(MagickFormat.Png);

        Mock.Setup(o => o.CreateChartAsync(It.IsAny<ChartRequestData>())).ReturnsAsync(bytes);
        return this;
    }

    public GraphicsClientBuilder SetVersionAction()
    {
        Mock.Setup(o => o.GetVersionAsync()).ReturnsAsync("1.0.0");
        return this;
    }

    public GraphicsClientBuilder SetGetMetricsAction()
    {
        Mock.Setup(o => o.GetMetricsAsync()).ReturnsAsync(new Metrics());
        return this;
    }

    public GraphicsClientBuilder SetGetStatisticsAction()
    {
        Mock.Setup(o => o.GetStatisticsAsync()).ReturnsAsync(new Stats());
        return this;
    }
}
