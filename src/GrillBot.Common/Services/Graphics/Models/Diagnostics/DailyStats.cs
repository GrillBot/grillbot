using Newtonsoft.Json.Converters;

namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class DailyStats
{
    public int Count { get; set; }
    public string Date { get; set; } = "";
}
