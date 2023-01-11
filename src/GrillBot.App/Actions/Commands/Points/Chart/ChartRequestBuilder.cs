using GrillBot.App.Services.Graphics.Models.Chart;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public static class ChartRequestBuilder
{
    public const int Height = 500;
    public const int Width = 1920;
    public const string Background = "white";
    
    public static ChartRequestData CreateCommonRequest()
    {
        return new ChartRequestData
        {
            Options = new ChartOptions
            {
                Height = Height,
                Width = Width,
                BackgroundColor = Background,
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
}
