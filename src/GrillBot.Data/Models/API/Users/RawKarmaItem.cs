using GrillBot.Core.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GrillBot.Data.Models.API.Users;

public class RawKarmaItem : IDictionaryObject
{
    [JsonProperty("member_ID")]
    [JsonPropertyName("member_ID")]
    [StringLength(32)]
    public string MemberId { get; set; } = null!;

    public int KarmaValue { get; set; }
    public int Positive { get; set; }
    public int Negative { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(MemberId), MemberId },
            { nameof(KarmaValue), KarmaValue.ToString() },
            { nameof(Positive), Positive.ToString() },
            { nameof(Negative), Negative.ToString() }
        };
    }
}
