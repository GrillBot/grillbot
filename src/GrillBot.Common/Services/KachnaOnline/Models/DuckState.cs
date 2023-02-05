
using System.Text.Json.Serialization;

namespace GrillBot.Common.Services.KachnaOnline.Models;

public class DuckState
{
    public int Id { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Enums.DuckState State { get; set; }

    public DuckUser MadeByUser { get; set; } = null!;

    public DateTime Start { get; set; }
    public DateTime? PlannedEnd { get; set; }

    public string? Note { get; set; }
    public int? EventId { get; set; }
    public DuckState? FollowingState { get; set; }
    public string? NoteInternal { get; set; }
}
