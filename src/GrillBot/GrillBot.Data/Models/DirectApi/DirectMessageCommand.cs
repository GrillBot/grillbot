using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.DirectApi;

public class DirectMessageCommand
{
    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; set; }

    public DirectMessageCommand() { }

    public DirectMessageCommand(string method)
    {
        Method = method;
        Parameters = new Dictionary<string, object>();
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }
}
