using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Chart;
using ImageMagick;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class GraphicsClientBuilder : BuilderBase<IGraphicsClient>
{
    public GraphicsClientBuilder SetAll()
        => SetCreateChartAction();
    
    public GraphicsClientBuilder SetCreateChartAction()
    {
        using var image = new MagickImage(MagickColors.White, 100, 100);
        new Drawables().Line(0, 50, 50, 0).Draw(image);
        var bytes = image.ToByteArray(MagickFormat.Png);

        Mock.Setup(o => o.CreateChartAsync(It.IsAny<ChartRequestData>())).ReturnsAsync(bytes);
        return this;
    }
}
