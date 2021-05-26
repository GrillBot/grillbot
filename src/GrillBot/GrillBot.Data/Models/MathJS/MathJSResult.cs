using Newtonsoft.Json;

namespace GrillBot.Data.Models.MathJS
{
    public class MathJSResult
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
