using GrillBot.Common.Services.Graphics.Models.Chart;

namespace GrillBot.Common.Services.ImageProcessing.Models;

public class ChartRequest
{
    public List<ChartRequestData> Requests { get; set; } = new();
}
