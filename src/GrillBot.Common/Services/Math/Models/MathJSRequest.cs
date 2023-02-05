using System.Text.Json.Serialization;

namespace GrillBot.Common.Services.Math.Models;

public class MathJsRequest
{
    [JsonPropertyName("expr")]
    public string Expression { get; set; } = null!;
}
