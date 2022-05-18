using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace GrillBot.Data.Models.Duck;

public class DuckState
{
    public int Id { get; set; }

    [JsonProperty(Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public Enums.DuckState State { get; set; }

    public DuckUser MadeByUser { get; set; }

    public DateTime Start { get; set; }
    public DateTime? PlannedEnd { get; set; }

    public string Note { get; set; }
    public int? EventId { get; set; }
    public DuckState FollowingState { get; set; }
    public string NoteInternal { get; set; }
}
