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

    public DirectMessageCommand()
    {
        Parameters = new Dictionary<string, object>();
    }

    public DirectMessageCommand(string method) : this()
    {
        Method = method;
    }

    public DirectMessageCommand WithParameter(string key, object value)
    {
        Parameters[key] = value;
        return this;
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null;
    }

    public override string ToString() => $"{Method}|{string.Join("|", Parameters.Values)}";
}
