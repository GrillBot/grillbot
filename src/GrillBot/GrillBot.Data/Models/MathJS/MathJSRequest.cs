using Newtonsoft.Json;

namespace GrillBot.Data.Models.MathJS
{
    public class MathJSRequest
    {
        [JsonProperty("expr", Required = Required.Always)]
        public string Expression { get; set; }
    }
}
