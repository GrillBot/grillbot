using GrillBot.Database.Enums;
using System;

namespace GrillBot.Data.Models.Suggestion;

public class SuggestionMetadata
{
    public string Id { get; set; }
    public SuggestionType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public object Data { get; set; }
}
