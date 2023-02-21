using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using GrillBot.Common.Infrastructure;
using Newtonsoft.Json;

namespace GrillBot.Common.Services.RubbergodService.Models.Karma;

public class KarmaItem : IApiObject
{
    [JsonPropertyName("member_ID")]
    [JsonProperty("member_ID")]
    [StringLength(32)]
    public string MemberId { get; set; } = null!;

    [JsonPropertyName("karma")]
    public int KarmaValue { get; set; }

    [JsonPropertyName("positive")]
    public int Positive { get; set; }

    [JsonPropertyName("negative")]
    public int Negative { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(MemberId), MemberId },
            { nameof(KarmaValue), KarmaValue.ToString() },
            { nameof(Positive), Positive.ToString() },
            { nameof(Negative), Negative.ToString() }
        };
    }
}
