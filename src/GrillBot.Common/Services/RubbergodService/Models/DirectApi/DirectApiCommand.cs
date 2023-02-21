using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace GrillBot.Common.Services.RubbergodService.Models.DirectApi;

public class DirectApiCommand
{
    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }

    public DirectApiCommand()
    {
        Method = "";
        Parameters = new Dictionary<string, object>();
    }

    public DirectApiCommand(string method) : this()
    {
        Method = method;
    }

    public DirectApiCommand WithParameter(string key, object value)
    {
        Parameters ??= new Dictionary<string, object>();
        Parameters[key] = value;
        return this;
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Parameters?.Count == 0) Parameters = null!;
    }

    public override string ToString() => $"{Method}|{(Parameters == null ? "" : string.Join("|", Parameters.Values))}";
}
