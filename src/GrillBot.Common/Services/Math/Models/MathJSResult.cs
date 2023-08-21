using System.Text.Json.Serialization;

namespace GrillBot.Common.Services.Math.Models;

public class MathJsResult
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonIgnore]
    public bool IsTimeout { get; set; }
}
