namespace GrillBot.Common.Services.Graphics.Models.Chart;

public class ChartData
{
    public Label? TopLabel { get; set; }
    public List<Dataset> Datasets { get; set; } = new();
}
