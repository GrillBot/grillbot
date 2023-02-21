using System.Linq.Expressions;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.Graphics.Models.Diagnostics;
using GrillBot.Common.Services.Graphics.Models.Images;
using ImageMagick;
using Moq;

namespace GrillBot.Tests.Infrastructure.Services;

public class GraphicsClientBuilder : BuilderBase<IGraphicsClient>
{
    public GraphicsClientBuilder SetAll()
        => SetVersionAction().SetGetMetricsAction().SetGetStatisticsAction().SetCreationMethods();

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

    public GraphicsClientBuilder SetCreationMethods()
    {
        using var image = new MagickImage(MagickColors.White, 100, 100);
        new Drawables().Line(0, 50, 50, 0).Draw(image);
        var bytes = image.ToByteArray(MagickFormat.Png);

        var singleFrameMethods = new Expression<Func<IGraphicsClient, Task<byte[]>>>[]
        {
            o => o.CreateChartAsync(It.IsAny<ChartRequestData>()),
            o => o.CreatePointsImageAsync(It.IsAny<PointsImageRequest>()),
            o => o.CreateWithoutAccidentImage(It.IsAny<WithoutAccidentRequestData>()),
        };

        var multipleFrameMethods = new Expression<Func<IGraphicsClient, Task<List<byte[]>>>>[]
        {
            o => o.CreatePeepoAngryAsync(It.IsAny<List<byte[]>>()),
            o => o.CreatePeepoLoveAsync(It.IsAny<List<byte[]>>())
        };

        foreach (var singleFrameMethod in singleFrameMethods)
            Mock.Setup(singleFrameMethod).ReturnsAsync(bytes);
        foreach (var multipleFrameMethod in multipleFrameMethods)
            Mock.Setup(multipleFrameMethod).ReturnsAsync(new List<byte[]> { bytes });
        return this;
    }
}
