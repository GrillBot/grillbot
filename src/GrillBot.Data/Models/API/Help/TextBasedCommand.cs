using Newtonsoft.Json;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.API.Help;

public class TextBasedCommand
{
    [JsonIgnore]
    [OpenApiIgnore]
    public string CommandId { get; set; }

    public string Command { get; set; }
    public List<string> Parameters { get; set; }
    public List<string> Aliases { get; set; }
    public string Description { get; set; }
    public List<string> Guilds { get; set; }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Guilds?.Count == 0) Guilds = null;
        if (Parameters?.Count == 0) Parameters = null;
        if (Aliases?.Count == 0) Aliases = null;
    }
}
