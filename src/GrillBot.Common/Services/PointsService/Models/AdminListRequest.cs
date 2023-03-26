using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;

namespace GrillBot.Common.Services.PointsService.Models;

public class AdminListRequest : IValidatableObject, IDictionaryObject
{
    [Required]
    public bool ShowMerged { get; set; }

    [StringLength(30)]
    [DiscordId]
    public string? GuildId { get; set; }

    [StringLength(30)]
    [DiscordId]
    public string? UserId { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public bool OnlyReactions { get; set; }
    public bool OnlyMessages { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? MessageId { get; set; }

    public PaginatedParams Pagination { get; set; } = new();
    public SortParameters Sort { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedFrom > CreatedTo)
            yield return new ValidationResult("Invalid interval From-To", new[] { nameof(CreatedFrom), nameof(CreatedTo) });
    }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(ShowMerged), ShowMerged.ToString() },
            { nameof(GuildId), GuildId },
            { nameof(UserId), UserId },
            { nameof(CreatedFrom), CreatedFrom?.ToString("o") },
            { nameof(CreatedTo), CreatedTo?.ToString("o") },
            { nameof(OnlyReactions), OnlyReactions.ToString() },
            { nameof(OnlyMessages), OnlyMessages.ToString() },
            { nameof(MessageId), MessageId }
        };

        result.MergeDictionaryObjects(Pagination, nameof(Pagination));
        result.MergeDictionaryObjects(Sort, nameof(Sort));
        return result;
    }
}
