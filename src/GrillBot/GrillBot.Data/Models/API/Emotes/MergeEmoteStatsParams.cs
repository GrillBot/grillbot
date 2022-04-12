using GrillBot.Data.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Emotes;

public class MergeEmoteStatsParams
{
    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string SourceEmoteId { get; set; }

    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string DestinationEmoteId { get; set; }
}
