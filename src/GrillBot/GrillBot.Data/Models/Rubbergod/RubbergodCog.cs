using System.Collections.Generic;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.Rubbergod;

public class RubbergodCog
{
    [JsonProperty("id")]
    public ulong? Id { get; set; }

    [JsonProperty("children")]
    public List<string> Children { get; set; } = new();
}
