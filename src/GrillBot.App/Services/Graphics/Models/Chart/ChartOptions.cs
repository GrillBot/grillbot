namespace GrillBot.App.Services.Graphics.Models.Chart;

public class ChartOptions
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string BackgroundColor { get; set; } = null!;
    public string Type { get; set; } = "line";
    public string LegendPosition { get; set; } = null!;
    public int? PointsRadius { get; set; }
}
