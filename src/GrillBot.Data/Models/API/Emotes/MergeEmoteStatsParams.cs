using System.Collections.Generic;
using GrillBot.Data.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Emotes;

public class MergeEmoteStatsParams : IApiObject
{
    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string SourceEmoteId { get; set; }

    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string DestinationEmoteId { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(SourceEmoteId), SourceEmoteId },
            { nameof(DestinationEmoteId), DestinationEmoteId }
        };
    }
}
