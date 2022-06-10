using Newtonsoft.Json;

namespace GrillBot.Data.Models.MathJS;

public class MathJsRequest
{
    [JsonProperty("expr", Required = Required.Always)]
    public string Expression { get; set; }
}
