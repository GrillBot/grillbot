using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;

namespace GrillBot.Data.Models.API.Emotes;

public class MergeEmoteStatsParams : IDictionaryObject
{
    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string SourceEmoteId { get; set; } = null!;

    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string DestinationEmoteId { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(SourceEmoteId), SourceEmoteId },
            { nameof(DestinationEmoteId), DestinationEmoteId }
        };
    }
}
