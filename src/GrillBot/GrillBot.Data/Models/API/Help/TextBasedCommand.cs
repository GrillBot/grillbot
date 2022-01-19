using Newtonsoft.Json;
using NSwag.Annotations;
using System.Collections.Generic;

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
}
