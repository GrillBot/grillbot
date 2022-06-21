using GrillBot.Data.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace GrillBot.Data.Models.API.Emotes;

public class MergeEmoteStatsParams
{
    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string SourceEmoteId { get; set; }

    [Required(ErrorMessage = "Pro sloučení je vyžadován EmoteId.")]
    [EmoteId(ErrorMessage = "EmoteId není ve správném formátu.")]
    public string DestinationEmoteId { get; set; }
    
    [JsonIgnore]
    [OpenApiIgnore]
    public bool SuppressValidations { get; set; }
}
