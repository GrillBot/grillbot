namespace GrillBot.App.Services.Graphics.Models.Chart;

public class ChartData
{
    public Label? TopLabel { get; set; }
    public List<string> Labels { get; set; } = new();
    public List<Dataset> Datasets { get; set; } = new();
}
