using System.Text.Json.Serialization;

namespace GrillBot.Common.Services.RubbergodService.Models.Help;

public class Cog
{
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    [JsonPropertyName("children")]
    public List<string> Children { get; set; } = new();
}
